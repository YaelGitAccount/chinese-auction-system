using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using server_NET.DTOs;
using server_NET.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftController : ControllerBase
    {
        private readonly IGiftService _giftService;
        private readonly ISystemStateService _systemStateService;
        private readonly ILogger<GiftController> _logger;
        
        public GiftController(IGiftService giftService, ISystemStateService systemStateService, ILogger<GiftController> logger)
        {
            _giftService = giftService;
            _systemStateService = systemStateService;
            _logger = logger;
        }

        // GET: api/gift
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? name, [FromQuery] string? category, [FromQuery] string? donorName, [FromQuery] int? buyers, [FromQuery] string? sortBy = "id")
        {
            try
            {
                _logger.LogInformation("Getting all gifts with filters - Name: {Name}, Category: {Category}, DonorName: {DonorName}, Buyers: {Buyers}, SortBy: {SortBy}", 
                    name ?? "None", category ?? "None", donorName ?? "None", buyers?.ToString() ?? "None", sortBy);
                
                var gifts = _giftService.GetAllGifts().ToList();
                if (_systemStateService.IsLotteryOccurred())
                {
                    // הצגת שם הזוכה לכל מתנה (אם יש)
                    foreach (var gift in gifts)
                    {
                        // הנחתה: WinnerName קיים ב-GiftDto או יש להוסיף אותו
                        gift.WinnerName = _giftService.GetWinnerNameForGift(gift.Id);
                    }
                }
                if (!string.IsNullOrEmpty(name))
                    gifts = gifts.Where(g => g.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(category))
                    gifts = gifts.Where(g => g.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(donorName))
                    gifts = gifts.Where(g => g.DonorName.Contains(donorName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (buyers.HasValue)
                    gifts = gifts.Where(g => g.BuyersCount == buyers.Value).ToList();
                gifts = sortBy?.ToLower() switch
                {
                    "name" => gifts.OrderBy(g => g.Name).ToList(),
                    "category" => gifts.OrderBy(g => g.CategoryName).ToList(),
                    _ => gifts.OrderBy(g => g.Id).ToList()
                };
                
                _logger.LogInformation("Successfully retrieved {Count} gifts", gifts.Count);
                return Ok(gifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving gifts with filters - Name: {Name}, Category: {Category}, DonorName: {DonorName}", 
                    name ?? "None", category ?? "None", donorName ?? "None");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // POST: api/gift
        [Authorize(Roles = "manager")]
        [HttpPost]
        public IActionResult Create([FromBody] CreateGiftDto createGiftDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createGiftDto.Name) || createGiftDto.Name.Length < 2)
                    return BadRequest("Gift name is required and must be at least 2 characters.");
                if (createGiftDto.CategoryId <= 0)
                    return BadRequest("Valid category ID is required.");
                if (createGiftDto.Price < 0)
                    return BadRequest("Gift price must be non-negative.");
                if (createGiftDto.DonorId <= 0)
                    return BadRequest("Valid donor ID is required.");
                var createdGift = _giftService.CreateGift(createGiftDto);
                return CreatedAtAction(nameof(GetById), new { id = createdGift.Id }, createdGift);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/gift/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var gift = _giftService.GetGiftById(id);
                if (gift == null)
                    return NotFound();
                return Ok(gift);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // PUT: api/gift/{id}
        [Authorize(Roles = "manager")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateGiftDto updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateDto.Name) || updateDto.Name.Length < 2)
                    return BadRequest("Gift name must be at least 2 characters.");
                if (updateDto.CategoryId <= 0)
                    return BadRequest("Valid category ID is required.");
                if (updateDto.Price < 0)
                    return BadRequest("Gift price must be non-negative.");
                if (updateDto.DonorId <= 0)
                    return BadRequest("Valid donor ID is required.");
                var updated = _giftService.UpdateGift(id, updateDto);
                if (!updated)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // DELETE: api/gift/{id}
        [Authorize(Roles = "manager")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var deleted = _giftService.DeleteGift(id);
                if (!deleted)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/gift/available - מתנות שעדיין ניתן לרכוש (ללא הגרלה)
        [HttpGet("available")]
        public IActionResult GetAvailable([FromQuery] string? name, [FromQuery] string? category, [FromQuery] string? donorName, [FromQuery] string? sortBy = "id")
        {
            try
            {
                _logger.LogInformation("Getting available gifts (without lottery) with filters - Name: {Name}, Category: {Category}, DonorName: {DonorName}, SortBy: {SortBy}", 
                    name ?? "None", category ?? "None", donorName ?? "None", sortBy);
                
                var gifts = _giftService.GetAvailableGifts().ToList();
                
                if (!string.IsNullOrEmpty(name))
                    gifts = gifts.Where(g => g.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(category))
                    gifts = gifts.Where(g => g.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!string.IsNullOrEmpty(donorName))
                    gifts = gifts.Where(g => g.DonorName.Contains(donorName, StringComparison.OrdinalIgnoreCase)).ToList();
                
                gifts = sortBy?.ToLower() switch
                {
                    "name" => gifts.OrderBy(g => g.Name).ToList(),
                    "category" => gifts.OrderBy(g => g.CategoryName).ToList(),
                    "price" => gifts.OrderBy(g => g.Price).ToList(),
                    _ => gifts.OrderBy(g => g.Id).ToList()
                };
                
                _logger.LogInformation("Successfully retrieved {Count} available gifts", gifts.Count);
                return Ok(gifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available gifts with filters - Name: {Name}, Category: {Category}, DonorName: {DonorName}", 
                    name ?? "None", category ?? "None", donorName ?? "None");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
