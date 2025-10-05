using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server_NET.DTOs;
using server_NET.Services;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotteryResultController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        private readonly ILogger<LotteryResultController> _logger;
        
        public LotteryResultController(ILotteryService lotteryService, ILogger<LotteryResultController> logger)
        {
            _lotteryService = lotteryService;
            _logger = logger;
        }

        // GET: api/LotteryResult
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Getting all lottery results");
            
            try
            {
                var results = _lotteryService.GetAllResults();
                
                _logger.LogInformation("Retrieved {Count} lottery results", results.Count());
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all lottery results");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/LotteryResult/gift/{giftId}
        [HttpGet("gift/{giftId}")]
        [AllowAnonymous]
        public IActionResult GetByGiftId(int giftId)
        {
            _logger.LogInformation("Getting lottery result for gift {GiftId}", giftId);
            
            try
            {
                if (giftId <= 0)
                {
                    _logger.LogWarning("Invalid gift ID provided: {GiftId}", giftId);
                    return BadRequest("Invalid gift id");
                }

                var result = _lotteryService.GetResultByGiftId(giftId);
                if (result == null)
                {
                    _logger.LogWarning("No lottery result found for gift {GiftId}", giftId);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved lottery result for gift {GiftId}", giftId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lottery result for gift {GiftId}", giftId);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // POST: api/LotteryResult
        [HttpPost]
        [Authorize(Roles = "manager")]
        public IActionResult Add([FromBody] LotteryResultDto dto)
        {
            if (dto == null || dto.GiftId <= 0 || dto.WinnerUserId <= 0)
                return BadRequest("Invalid data");
            try
            {
                var result = _lotteryService.CreateLotteryResult(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lottery result");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // PUT: api/LotteryResult/{giftId}
        [HttpPut("{giftId}")]
        [Authorize(Roles = "manager")]
        public IActionResult Update(int giftId, [FromBody] LotteryResultDto dto)
        {
            if (giftId <= 0 || dto == null || dto.WinnerUserId <= 0)
                return BadRequest("Invalid data");
            try
            {
                var result = _lotteryService.UpdateLotteryResult(giftId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lottery result");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // DELETE: api/LotteryResult/{giftId}
        [HttpDelete("{giftId}")]
        [Authorize(Roles = "manager")]
        public IActionResult Delete(int giftId)
        {
            if (giftId <= 0) return BadRequest("Invalid gift id");
            try
            {
                var deleted = _lotteryService.DeleteLotteryResult(giftId);
                if (!deleted)
                    return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lottery result");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // POST: api/LotteryResult/draw-all
        [HttpPost("draw-all")]
        [Authorize(Roles = "manager")]
        public IActionResult DrawAllLotteries()
        {
            try
            {
                _logger.LogInformation("Drawing lottery for all gifts");
                var result = _lotteryService.DrawAllLotteries();
                _logger.LogInformation("Successfully completed lottery draw for all gifts");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error drawing lottery for all gifts");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/LotteryResult/export
        [HttpGet("export")]
        [Authorize(Roles = "manager")]
        public IActionResult ExportWinners()
        {
            try
            {
                _logger.LogInformation("Exporting lottery winners to Excel");
                
                var results = _lotteryService.GetAllResults().ToList();
                
                if (!results.Any())
                {
                    return BadRequest("No lottery results to export");
                }

                // This would need to be implemented with a proper Excel generation library
                // For now, return JSON data that could be converted to Excel on the client side
                var exportData = results.Select(r => new {
                    GiftName = r.GiftName,
                    CategoryName = r.CategoryName,
                    GiftPrice = r.GiftPrice,
                    WinnerName = r.WinnerName,
                    WinnerEmail = r.WinnerEmail,
                    DrawDate = r.DrawDate,
                    ParticipantsCount = r.ParticipantsCount
                }).ToList();

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting lottery winners");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
