using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server_NET.Services;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "manager")]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _lotteryService;
        private readonly ILogger<LotteryController> _logger;

        public LotteryController(ILotteryService lotteryService, ILogger<LotteryController> logger)
        {
            _lotteryService = lotteryService;
            _logger = logger;
        }

        // קבלת סיכום ההגרלה
        [HttpGet("summary")]
        public IActionResult GetLotterySummary()
        {
            try
            {
                var summary = _lotteryService.GetLotterySummary();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lottery summary");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת סיכום ההגרלה - נגיש לכולם
        [AllowAnonymous]
        [HttpGet("public-summary")]
        public IActionResult GetPublicLotterySummary()
        {
            try
            {
                var summary = _lotteryService.GetLotterySummary();
                // Return only public info
                return Ok(new
                {
                    completedLotteries = summary["completedLotteries"],
                    awaitingLotteries = summary["awaitingLotteries"],
                    totalRevenue = summary["totalRevenue"],
                    isCompleted = summary["isCompleted"]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public lottery summary");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת כל תוצאות ההגרלה
        [HttpGet("results")]
        public IActionResult GetAllResults()
        {
            try
            {
                var results = _lotteryService.GetAllResults();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lottery results");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת מתנות שממתינות להגרלה
        [HttpGet("pending-gifts")]
        public IActionResult GetGiftsAwaitingLottery()
        {
            try
            {
                var gifts = _lotteryService.GetGiftsAwaitingLottery();
                return Ok(gifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gifts awaiting lottery");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // הגרלה למתנה ספציפית
        [HttpPost("draw/{giftId}")]
        public IActionResult DrawLottery(int giftId)
        {
            try
            {
                if (giftId <= 0)
                    return BadRequest("Invalid gift ID.");

                _lotteryService.DrawLottery(giftId);
                
                // קבלת תוצאת ההגרלה
                var result = _lotteryService.GetResultByGiftId(giftId);
                
                _logger.LogInformation("Lottery drawn successfully for gift {GiftId}", giftId);
                return Ok(new { 
                    message = "Lottery drawn successfully", 
                    result = result 
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid lottery operation for gift {GiftId}: {Message}", giftId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error drawing lottery for gift {GiftId}", giftId);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת תוצאת הגרלה למתנה ספציפית
        [HttpGet("result/{giftId}")]
        public IActionResult GetResultByGiftId(int giftId)
        {
            try
            {
                if (giftId <= 0)
                    return BadRequest("Invalid gift ID.");

                var result = _lotteryService.GetResultByGiftId(giftId);
                if (result == null)
                    return NotFound("No lottery result found for this gift.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lottery result for gift {GiftId}", giftId);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת סך ההכנסות
        [HttpGet("revenue")]
        public IActionResult GetTotalRevenue()
        {
            try
            {
                var revenue = _lotteryService.GetTotalRevenue();
                return Ok(new { totalRevenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total revenue");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // בדיקה אם ההגרלה הושלמה
        [HttpGet("is-completed")]
        public IActionResult IsLotteryCompleted()
        {
            try
            {
                var isCompleted = _lotteryService.IsLotteryCompleted();
                return Ok(new { isCompleted = isCompleted });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking lottery completion status");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת כל תוצאות ההגרלה - נגיש לכל המשתמשים
        [AllowAnonymous]
        [HttpGet("public-results")]
        public IActionResult GetPublicResults()
        {
            try
            {
                var results = _lotteryService.GetAllResults();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public lottery results");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // בדיקת שליחת מייל (למנהלים בלבד)
        [Authorize(Roles = "manager")]
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email is required");

                var emailHelper = HttpContext.RequestServices.GetService<server_NET.Helpers.EmailHelper>();
                if (emailHelper == null)
                    return StatusCode(500, "Email service not available");

                var result = await emailHelper.SendTestEmailAsync(email);
                
                if (result)
                {
                    _logger.LogInformation("Test email sent successfully to {Email}", email);
                    return Ok(new { message = "Test email sent successfully", email = email });
                }
                else
                {
                    _logger.LogWarning("Failed to send test email to {Email}", email);
                    return StatusCode(500, "Failed to send test email");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email to {Email}", email);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
