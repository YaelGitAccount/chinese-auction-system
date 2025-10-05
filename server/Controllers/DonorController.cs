using Microsoft.AspNetCore.Mvc;
using server_NET.Services;
using server_NET.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "manager")]
    public class DonorController : ControllerBase
    {
        private readonly IDonorService _donorService;
        private readonly ILogger<DonorController> _logger;

        public DonorController(IDonorService donorService, ILogger<DonorController> logger)
        {
            _donorService = donorService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] string? name, [FromQuery] string? email, [FromQuery] string? gift)
        {
            try
            {
                _logger.LogInformation("Getting all donors with filters - Name: {Name}, Email: {Email}, Gift: {Gift}", 
                    name ?? "None", email ?? "None", gift ?? "None");
                
                var donors = _donorService.GetAllDonors();
                if (!string.IsNullOrEmpty(email))
                    donors = donors.Where(d => d.Email.Contains(email, StringComparison.OrdinalIgnoreCase));
                else if (!string.IsNullOrEmpty(gift))
                    donors = donors.Where(d => d.Gifts.Any(g => g.Name.Contains(gift, StringComparison.OrdinalIgnoreCase)));
                else if (!string.IsNullOrEmpty(name))
                    donors = donors.Where(d => d.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
                // ברירת מחדל: מיון לפי שם
                donors = donors.OrderBy(d => d.Name);
                
                var result = donors.ToList();
                _logger.LogInformation("Successfully retrieved {Count} donors", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donors with filters - Name: {Name}, Email: {Email}, Gift: {Gift}", 
                    name ?? "None", email ?? "None", gift ?? "None");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                _logger.LogInformation("Getting donor by ID: {DonorId}", id);
                
                var donor = _donorService.GetDonorById(id);
                if (donor == null)
                {
                    _logger.LogWarning("Donor not found with ID: {DonorId}", id);
                    return NotFound();
                }
                
                _logger.LogInformation("Successfully retrieved donor: {DonorName} (ID: {DonorId})", donor.Name, id);
                return Ok(donor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donor by ID: {DonorId}", id);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateDonorDto createDonorDto)
        {
            try
            {
                _logger.LogInformation("Creating new donor: {DonorName}, Email: {Email}", 
                    createDonorDto.Name, createDonorDto.Email);
                
                if (string.IsNullOrWhiteSpace(createDonorDto.Name))
                {
                    _logger.LogWarning("Donor creation failed - Name is required");
                    return BadRequest("Donor name is required.");
                }
                if (string.IsNullOrWhiteSpace(createDonorDto.Email))
                {
                    _logger.LogWarning("Donor creation failed - Email is required");
                    return BadRequest("Donor email is required.");
                }
                if (string.IsNullOrWhiteSpace(createDonorDto.Phone))
                {
                    _logger.LogWarning("Donor creation failed - Phone is required");
                    return BadRequest("Donor phone is required.");
                }
                
                var createdDonor = _donorService.CreateDonor(new DonorDto {
                    Name = createDonorDto.Name,
                    Email = createDonorDto.Email,
                    Phone = createDonorDto.Phone
                });
                
                _logger.LogInformation("Successfully created donor: {DonorName} (ID: {DonorId})", 
                    createdDonor.Name, createdDonor.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdDonor.Id }, createdDonor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donor: {DonorName}, Email: {Email}", 
                    createDonorDto.Name, createDonorDto.Email);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateDonorDto updateDonorDto)
        {
            try
            {
                _logger.LogInformation("Updating donor with ID: {DonorId}", id);
                
                if (updateDonorDto == null)
                {
                    _logger.LogWarning("Donor update failed - No data provided for ID: {DonorId}", id);
                    return BadRequest("No data provided.");
                }
                if (updateDonorDto.Name != null && string.IsNullOrWhiteSpace(updateDonorDto.Name))
                {
                    _logger.LogWarning("Donor update failed - Name cannot be empty for ID: {DonorId}", id);
                    return BadRequest("Donor name cannot be empty.");
                }
                if (updateDonorDto.Email != null && string.IsNullOrWhiteSpace(updateDonorDto.Email))
                {
                    _logger.LogWarning("Donor update failed - Email cannot be empty for ID: {DonorId}", id);
                    return BadRequest("Donor email cannot be empty.");
                }
                if (updateDonorDto.Phone != null && string.IsNullOrWhiteSpace(updateDonorDto.Phone))
                {
                    _logger.LogWarning("Donor update failed - Phone cannot be empty for ID: {DonorId}", id);
                    return BadRequest("Donor phone cannot be empty.");
                }
                
                var donor = _donorService.GetDonorById(id);
                if (donor == null)
                {
                    _logger.LogWarning("Donor update failed - Donor not found with ID: {DonorId}", id);
                    return NotFound();
                }
                
                var donorDto = new DonorDto {
                    Id = donor.Id,
                    Name = updateDonorDto.Name ?? donor.Name,
                    Email = updateDonorDto.Email ?? donor.Email,
                    Phone = updateDonorDto.Phone ?? donor.Phone,
                    Gifts = updateDonorDto.Gifts ?? donor.Gifts
                };
                
                var updated = _donorService.UpdateDonor(id, donorDto);
                if (!updated)
                {
                    _logger.LogWarning("Donor update failed - Update operation returned false for ID: {DonorId}", id);
                    return NotFound();
                }
                
                _logger.LogInformation("Successfully updated donor with ID: {DonorId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating donor with ID: {DonorId}", id);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete donor with ID: {DonorId}", id);
                
                var deleted = _donorService.DeleteDonor(id);
                if (!deleted)
                {
                    _logger.LogWarning("Donor deletion failed - Donor not found with ID: {DonorId}", id);
                    return NotFound();
                }
                
                _logger.LogInformation("Successfully deleted donor with ID: {DonorId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting donor with ID: {DonorId}", id);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
