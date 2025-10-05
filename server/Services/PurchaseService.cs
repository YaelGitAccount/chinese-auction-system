using server_NET.DTOs;
using server_NET.Models;
using server_NET.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace server_NET.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGiftRepository _giftRepository;
        private readonly ILotteryRepository _lotteryRepository;
        private readonly ILogger<PurchaseService> _logger;
        
        public PurchaseService(IPurchaseRepository purchaseRepository, IUserRepository userRepository, IGiftRepository giftRepository, ILotteryRepository lotteryRepository, ILogger<PurchaseService> logger)
        {
            _purchaseRepository = purchaseRepository;
            _userRepository = userRepository;
            _giftRepository = giftRepository;
            _lotteryRepository = lotteryRepository;
            _logger = logger;
        }

        public IEnumerable<PurchaseDto> GetAll()
        {
            try
            {
                return _purchaseRepository.GetAll().Select(MapToDto);
            }
            catch
            {
                return Enumerable.Empty<PurchaseDto>();
            }
        }
        public PurchaseDto? GetById(int id)
        {
            if (id <= 0) return null;
            try
            {
                return MapToDto(_purchaseRepository.GetById(id));
            }
            catch
            {
                return null;
            }
        }
        public void Add(PurchaseDto purchase)
        {
            if (purchase == null || purchase.UserId <= 0 || purchase.GiftId <= 0 || string.IsNullOrWhiteSpace(purchase.Status))
                throw new ArgumentException("Invalid purchase data");
            try
            {
                var user = _userRepository.GetById(purchase.UserId);
                var gift = _giftRepository.GetById(purchase.GiftId);
                if (user == null || gift == null)
                    throw new ArgumentException("User or Gift not found");
                var entity = new Purchase { User = user, Gift = gift, Status = purchase.Status };
                _purchaseRepository.Add(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add purchase: {ex.Message}");
            }
        }
        public void Update(PurchaseDto purchase)
        {
            if (purchase == null || purchase.Id <= 0 || purchase.UserId <= 0 || purchase.GiftId <= 0 || string.IsNullOrWhiteSpace(purchase.Status))
                throw new ArgumentException("Invalid purchase data");
            try
            {
                var entity = _purchaseRepository.GetById(purchase.Id);
                if (entity == null)
                    throw new ArgumentException("Purchase not found");
                entity.Status = purchase.Status;
                _purchaseRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update purchase: {ex.Message}");
            }
        }
        public void Delete(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid purchase id");
            try
            {
                _purchaseRepository.Delete(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete purchase: {ex.Message}");
            }
        }
        public bool AddGiftToCart(int userId, int giftId)
        {
            _logger.LogInformation("Adding gift {GiftId} to cart for user {UserId}", giftId, userId);
            
            if (userId <= 0 || giftId <= 0) 
            {
                _logger.LogWarning("Invalid parameters - UserId: {UserId}, GiftId: {GiftId}", userId, giftId);
                return false;
            }
            
            try
            {
                var user = _userRepository.GetById(userId);
                var gift = _giftRepository.GetById(giftId);
                if (user == null || gift == null)
                {
                    _logger.LogWarning("User or gift not found - UserId: {UserId}, GiftId: {GiftId}", userId, giftId);
                    return false;
                }
                
                // Check if lottery already completed for this gift
                if (gift.IsLotteryCompleted)
                {
                    _logger.LogWarning("Cannot add gift {GiftId} to cart - lottery already completed for this gift", giftId);
                    return false;
                }
                
                var existing = _purchaseRepository.GetAll().FirstOrDefault(p => p.User != null && p.Gift != null && p.User.Id == userId && p.Gift.Id == giftId && p.Status == "cart");
                if (existing != null)
                {
                    existing.Quantity++;
                    _purchaseRepository.Update(existing);
                    _logger.LogInformation("Updated existing cart item quantity to {Quantity} for gift {GiftId} and user {UserId}", existing.Quantity, giftId, userId);
                }
                else
                {
                    var purchase = new Purchase { User = user, Gift = gift, Status = "cart", Quantity = 1 };
                    _purchaseRepository.Add(purchase);
                    _logger.LogInformation("Added new cart item for gift {GiftId} and user {UserId}", giftId, userId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding gift {GiftId} to cart for user {UserId}", giftId, userId);
                return false;
            }
        }
        public bool Checkout(int userId)
        {
            _logger.LogInformation("Starting checkout process for user {UserId}", userId);
            
            if (userId <= 0) 
            {
                _logger.LogWarning("Invalid user ID for checkout: {UserId}", userId);
                return false;
            }
            
            try
            {
                var user = _userRepository.GetById(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for checkout: {UserId}", userId);
                    return false;
                }
                
                var cartPurchases = _purchaseRepository.GetAll().Where(p => p.User != null && p.Gift != null && p.User.Id == userId && p.Status == "cart").ToList();
                if (!cartPurchases.Any())
                {
                    _logger.LogWarning("No cart items found for user {UserId}", userId);
                    return false;
                }
                
                _logger.LogInformation("Processing {Count} cart items for user {UserId}", cartPurchases.Count, userId);
                
                foreach (var purchase in cartPurchases)
                {
                    purchase.Status = "paid";
                    purchase.PurchaseDate = DateTime.Now;
                    _purchaseRepository.Update(purchase);
                }
                
                _logger.LogInformation("Checkout completed successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user {UserId}", userId);
                return false;
            }
        }

        public string RemoveFromCart(int userId, int purchaseId)
        {
            if (userId <= 0 || purchaseId <= 0) 
                return "error";
            
            try
            {
                var purchase = _purchaseRepository.GetById(purchaseId);
                if (purchase == null)
                    return "not_found";
                
                if (purchase.User?.Id != userId)
                    return "not_found";
                
                if (purchase.Status != "cart")
                    return "not_cart";

                _purchaseRepository.Delete(purchaseId);
                return "success";
            }
            catch
            {
                return "error";
            }
        }

        public string UpdateCartQuantity(int userId, int purchaseId, string action)
        {
            _logger.LogInformation("Updating cart quantity for purchase {PurchaseId}, user {UserId}, action: {Action}", purchaseId, userId, action);
            
            if (userId <= 0 || purchaseId <= 0) 
            {
                _logger.LogWarning("Invalid parameters - UserId: {UserId}, PurchaseId: {PurchaseId}", userId, purchaseId);
                return "error";
            }
            
            if (string.IsNullOrWhiteSpace(action) || (action != "increase" && action != "decrease"))
            {
                _logger.LogWarning("Invalid action parameter: {Action}", action);
                return "error";
            }
            
            try
            {
                var purchase = _purchaseRepository.GetById(purchaseId);
                if (purchase == null)
                {
                    _logger.LogWarning("Purchase not found: {PurchaseId}", purchaseId);
                    return "not_found";
                }
                
                if (purchase.User?.Id != userId)
                {
                    _logger.LogWarning("User mismatch for purchase {PurchaseId} - expected: {ExpectedUserId}, actual: {ActualUserId}", purchaseId, userId, purchase.User?.Id);
                    return "not_found";
                }
                
                if (purchase.Status != "cart")
                {
                    _logger.LogWarning("Purchase {PurchaseId} is not in cart status: {Status}", purchaseId, purchase.Status);
                    return "not_cart";
                }

                if (action == "increase")
                {
                    purchase.Quantity++;
                    _logger.LogDebug("Increased quantity to {Quantity} for purchase {PurchaseId}", purchase.Quantity, purchaseId);
                }
                else if (action == "decrease")
                {
                    if (purchase.Quantity <= 1)
                    {
                        _logger.LogWarning("Cannot decrease quantity below 1 for purchase {PurchaseId}, current: {Quantity}", purchaseId, purchase.Quantity);
                        return "min_quantity";
                    }
                    purchase.Quantity--;
                    _logger.LogDebug("Decreased quantity to {Quantity} for purchase {PurchaseId}", purchase.Quantity, purchaseId);
                }

                _purchaseRepository.Update(purchase);
                _logger.LogInformation("Successfully updated cart quantity for purchase {PurchaseId}", purchaseId);
                return "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity for purchase {PurchaseId}", purchaseId);
                return "error";
            }
        }

        private PurchaseDto MapToDto(Purchase? p)
        {
            if (p == null || p.User == null || p.Gift == null) return new PurchaseDto();
            
            // Check if lottery result exists for this gift
            bool isGiftLotteryCompleted = p.Gift.IsLotteryCompleted;
            
            return new PurchaseDto
            {
                Id = p.Id,
                UserId = p.User?.Id ?? 0,
                GiftId = p.Gift?.Id ?? 0,
                Quantity = p.Quantity,
                PurchaseDate = p.PurchaseDate,
                Status = p.Status,
                GiftName = p.Gift?.Name ?? string.Empty,
                GiftPrice = p.Gift?.Price ?? 0,
                BuyerName = p.User?.Name ?? string.Empty,
                IsGiftLotteryCompleted = isGiftLotteryCompleted
            };
        }
    }
}
