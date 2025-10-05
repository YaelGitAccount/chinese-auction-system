using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server_NET.Services;
using System.Security.Claims;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ISystemStateService _systemStateService;
        private readonly ILogger<PurchaseController> _logger;
        
        public PurchaseController(IPurchaseService purchaseService, ISystemStateService systemStateService, ILogger<PurchaseController> logger)
        {
            _purchaseService = purchaseService;
            _systemStateService = systemStateService;
            _logger = logger;
        }

        // Add gift to cart
        [Authorize(Roles = "customer,manager")]
        [HttpPost("cart/{giftId}")]
        public IActionResult AddToCart(int giftId)
        {
            _logger.LogInformation("Attempting to add gift {GiftId} to cart", giftId);
            
            try
            {
                if (_systemStateService.IsLotteryOccurred())
                {
                    _logger.LogWarning("Attempt to add gift {GiftId} to cart after lottery occurred", giftId);
                    return StatusCode(403, "Cannot add to cart: The lottery has already occurred.");
                }
                
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    _logger.LogWarning("Add to cart attempt without valid user ID in token");
                    return Unauthorized("User ID not found in token.");
                }
                
                var userId = int.Parse(userIdStr);
                var result = _purchaseService.AddGiftToCart(userId, giftId);
                if (!result)
                {
                    _logger.LogWarning("Failed to add gift {GiftId} to cart for user {UserId}", giftId, userId);
                    return BadRequest("Cannot add gift to cart - lottery may have already been drawn for this gift or the gift does not exist.");
                }
                
                _logger.LogInformation("Gift {GiftId} successfully added to cart for user {UserId}", giftId, userId);
                return Ok("Gift added to cart.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding gift {GiftId} to cart", giftId);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // Purchase entire cart
        [Authorize(Roles = "customer,manager")]
        [HttpPost("checkout")]
        public IActionResult Checkout()
        {
            _logger.LogInformation("Checkout attempt started");
            
            try
            {
                if (_systemStateService.IsLotteryOccurred())
                {
                    _logger.LogWarning("Checkout attempt after lottery occurred");
                    return StatusCode(403, "Cannot checkout: The lottery has already occurred.");
                }
                
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    _logger.LogWarning("Checkout attempt without valid user ID in token");
                    return Unauthorized("User ID not found in token.");
                }
                
                var userId = int.Parse(userIdStr);
                var result = _purchaseService.Checkout(userId);
                if (!result)
                {
                    _logger.LogWarning("Checkout failed for user {UserId} - no items or error occurred", userId);
                    return BadRequest("No items to purchase or error occurred.");
                }
                
                _logger.LogInformation("Checkout completed successfully for user {UserId}", userId);
                return Ok("Purchase completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/Purchase/cart
        [Authorize(Roles = "customer,manager")]
        [HttpGet("cart")]
        public IActionResult GetCart()
        {
            _logger.LogDebug("Getting cart contents");
            
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    _logger.LogWarning("Get cart attempt without valid user ID in token");
                    return Unauthorized("User ID not found in token.");
                }
                
                var userId = int.Parse(userIdStr);
                var purchases = _purchaseService.GetAll()
                    .Where(p => p.UserId == userId && p.Status == "cart")
                    .ToList();
                
                _logger.LogDebug("Retrieved {Count} cart items for user {UserId}", purchases.Count, userId);
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart contents");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/Purchase/orders
        [Authorize(Roles = "customer,manager")]
        [HttpGet("orders")]
        public IActionResult GetOrders()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID not found in token.");
                var userId = int.Parse(userIdStr);
                var purchases = _purchaseService.GetAll()
                    .Where(p => p.UserId == userId && p.Status == "paid")
                    .ToList();
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/Purchase/all - Manager only
        [Authorize(Roles = "manager")]
        [HttpGet("all")]
        public IActionResult GetAllPurchases([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "name")
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;
                var purchases = _purchaseService.GetAll();

                // Summary by gift
                var summary = purchases
                    .GroupBy(p => new { p.GiftId, p.GiftName, p.GiftPrice })
                    .Select(g => new {
                        giftId = g.Key.GiftId,
                        giftName = g.Key.GiftName,
                        giftPrice = g.Key.GiftPrice,
                        totalQuantity = g.Sum(x => x.Quantity),
                        buyersCount = g.Select(x => x.UserId).Distinct().Count()
                    });

                // Sorting
                switch (sortBy.ToLower())
                {
                    case "price":
                    case "price-desc":
                        summary = summary.OrderByDescending(s => s.giftPrice);
                        break;
                    case "price-asc":
                        summary = summary.OrderBy(s => s.giftPrice);
                        break;
                    case "popularity":
                        summary = summary.OrderByDescending(s => s.totalQuantity);
                        break;
                    default:
                        summary = summary.OrderBy(s => s.giftName ?? string.Empty);
                        break;
                }

                // Pagination
                var total = summary.Count();
                var paged = summary.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var result = new {
                    total,
                    page,
                    pageSize,
                    data = paged
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // Overall summary report for managers only
        [Authorize(Roles = "manager")]
        [HttpGet("summary/overall")]
        public IActionResult GetOverallSummary()
        {
            try
            {
                var purchases = _purchaseService.GetAll().Where(p => p.Status == "paid");
                var totalIncome = purchases.Sum(p => p.GiftPrice * p.Quantity);
                var totalPurchases = purchases.Count();
                var totalUsers = purchases.Select(p => p.UserId).Distinct().Count();
                var totalGifts = purchases.Select(p => p.GiftId).Distinct().Count();
                
                return Ok(new {
                    totalIncome = totalIncome,
                    totalPurchases = totalPurchases,
                    totalUsers = totalUsers,
                    totalGifts = totalGifts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // Income report by gift
        [Authorize(Roles = "manager")]
        [HttpGet("summary/by-gift")]
        public IActionResult GetIncomeByGift()
        {
            try
            {
                var purchases = _purchaseService.GetAll().Where(p => p.Status == "paid");
                var summary = purchases
                    .GroupBy(p => new { p.GiftId, p.GiftName, p.GiftPrice })
                    .Select(g => new {
                        giftId = g.Key.GiftId,
                        giftName = g.Key.GiftName,
                        giftPrice = g.Key.GiftPrice,
                        totalQuantity = g.Sum(x => x.Quantity),
                        totalIncome = g.Sum(x => x.Quantity * x.GiftPrice),
                        buyersCount = g.Select(x => x.UserId).Distinct().Count()
                    })
                    .OrderByDescending(s => s.totalIncome)
                    .ToList();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/Purchase/by-gift/{giftId} - Manager only
        [Authorize(Roles = "manager")]
        [HttpGet("by-gift/{giftId}")]
        public IActionResult GetPurchasesByGift(int giftId)
        {
            try
            {
                if (giftId <= 0)
                    return BadRequest("Invalid gift id");
                var purchases = _purchaseService.GetAll()
                    .Where(p => p.GiftId == giftId && p.Status == "paid")
                    .ToList();
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // הסרת מתנה מהסל
        [Authorize(Roles = "customer,manager")]
        [HttpDelete("cart/{purchaseId}")]
        public IActionResult RemoveFromCart(int purchaseId)
        {
            _logger.LogInformation("Attempting to remove purchase {PurchaseId} from cart", purchaseId);
            
            try
            {
                if (_systemStateService.IsLotteryOccurred())
                {
                    _logger.LogWarning("Attempt to remove from cart after lottery occurred");
                    return StatusCode(403, "Cannot remove from cart: The lottery has already occurred.");
                }
                
                if (purchaseId <= 0)
                {
                    _logger.LogWarning("Invalid purchase ID provided: {PurchaseId}", purchaseId);
                    return BadRequest("Invalid purchase ID.");
                }
                
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                {
                    _logger.LogWarning("Remove from cart attempt without valid user ID in token");
                    return Unauthorized("User ID not found in token.");
                }
                
                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                {
                    _logger.LogWarning("Invalid user ID in token: {UserId}", userIdStr);
                    return Unauthorized("Invalid user ID in token.");
                }
                
                var result = _purchaseService.RemoveFromCart(userId, purchaseId);
                
                switch (result)
                {
                    case "success":
                        _logger.LogInformation("Successfully removed purchase {PurchaseId} from cart for user {UserId}", purchaseId, userId);
                        return Ok("Item removed from cart.");
                    case "not_found":
                        _logger.LogWarning("Purchase {PurchaseId} not found or doesn't belong to user {UserId}", purchaseId, userId);
                        return NotFound("Purchase item not found or doesn't belong to you.");
                    case "not_cart":
                        _logger.LogWarning("Attempt to remove non-cart item {PurchaseId} by user {UserId}", purchaseId, userId);
                        return BadRequest("Can only remove items in cart status.");
                    case "error":
                    default:
                        _logger.LogError("Failed to remove purchase {PurchaseId} from cart for user {UserId}", purchaseId, userId);
                        return BadRequest("Failed to remove item from cart.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing purchase {PurchaseId} from cart", purchaseId);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // עדכון כמות במתנה בסל - הוספה או הפחתה של 1
        [Authorize(Roles = "customer,manager")]
        [HttpPut("cart/{purchaseId}/quantity")]
        public IActionResult UpdateCartQuantity(int purchaseId, [FromQuery] string action)
        {
            try
            {
                if (_systemStateService.IsLotteryOccurred())
                    return StatusCode(403, "Cannot update cart: The lottery has already occurred.");
                
                if (purchaseId <= 0)
                    return BadRequest("Invalid purchase ID.");
                
                if (string.IsNullOrWhiteSpace(action) || (action != "increase" && action != "decrease"))
                    return BadRequest("Action must be 'increase' or 'decrease'.");
                
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized("User ID not found in token.");
                
                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                    return Unauthorized("Invalid user ID in token.");
                
                var result = _purchaseService.UpdateCartQuantity(userId, purchaseId, action);
                
                switch (result)
                {
                    case "success":
                        return Ok("Quantity updated successfully.");
                    case "not_found":
                        return NotFound("Purchase item not found or doesn't belong to you.");
                    case "not_cart":
                        return BadRequest("Can only update items in cart status.");
                    case "min_quantity":
                        return BadRequest("Cannot decrease quantity below 1. Use remove instead.");
                    case "error":
                    default:
                        return BadRequest("Failed to update quantity.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
