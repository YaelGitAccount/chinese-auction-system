using server_NET.DTOs;
using server_NET.Models;
using server_NET.Helpers;
using server_NET.Repositories;

namespace server_NET.Services
{
    public class LotteryService : ILotteryService
    {
        private readonly ILotteryRepository _lotteryRepository;
        private readonly IGiftRepository _giftRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISystemStateService _systemStateService;
        private readonly ILogger<LotteryService> _logger;
        private readonly EmailHelper _emailHelper;
        // Removed: private readonly Random _random; 
        // Using SecureRandom.Next() for cryptographically secure lottery draws

        public LotteryService(
            ILotteryRepository lotteryRepository,
            IGiftRepository giftRepository,
            IUserRepository userRepository,
            IPurchaseRepository purchaseRepository,
            ISystemStateService systemStateService,
            ILogger<LotteryService> logger, 
            EmailHelper emailHelper)
        {
            _lotteryRepository = lotteryRepository;
            _giftRepository = giftRepository;
            _userRepository = userRepository;
            _purchaseRepository = purchaseRepository;
            _systemStateService = systemStateService;
            _logger = logger;
            _emailHelper = emailHelper;
            // Note: Using SecureRandom static class instead of instance Random
        }

        public IEnumerable<LotteryResultDto> GetAllResults()
        {
            var results = _lotteryRepository.GetAll().ToList();
            
            // Performance optimization: Get ticket counts for all completed lotteries in single query
            var giftIds = results.Select(lr => lr.GiftId);
            var ticketCounts = _purchaseRepository.GetTicketCountsByGiftIds(giftIds);
            
            return results.Select(lr => new LotteryResultDto
            {
                Id = lr.Id,
                GiftId = lr.GiftId,
                GiftName = lr.Gift?.Name ?? "Unknown",
                GiftPrice = lr.Gift?.Price ?? 0,
                CategoryName = lr.Gift?.Category?.Name ?? "Unknown",
                WinnerUserId = lr.WinnerUserId,
                WinnerName = lr.WinnerUser?.Name ?? "Unknown",
                WinnerEmail = lr.WinnerUser?.Email ?? "Unknown",
                DrawDate = lr.Date,
                ParticipantsCount = ticketCounts.GetValueOrDefault(lr.GiftId, 0)
            })
            .OrderBy(lr => lr.DrawDate)
            .ToList();
        }

        public LotteryResultDto? GetResultByGiftId(int giftId)
        {
            var result = _lotteryRepository.GetByGiftId(giftId);
            if (result == null) return null;

            // Performance optimization: Get ticket count with aggregate query
            var ticketCounts = _purchaseRepository.GetTicketCountsByGiftIds(new[] { giftId });

            return new LotteryResultDto
            {
                Id = result.Id,
                GiftId = result.GiftId,
                GiftName = result.Gift?.Name ?? "Unknown",
                GiftPrice = result.Gift?.Price ?? 0,
                CategoryName = result.Gift?.Category?.Name ?? "Unknown",
                WinnerUserId = result.WinnerUserId,
                WinnerName = result.WinnerUser?.Name ?? "Unknown",
                WinnerEmail = result.WinnerUser?.Email ?? "Unknown",
                DrawDate = result.Date,
                ParticipantsCount = ticketCounts.GetValueOrDefault(giftId, 0)
            };
        }

        public async Task DrawLottery(int giftId)
        {
            if (_lotteryRepository.GetByGiftId(giftId) != null)
            {
                throw new InvalidOperationException($"Lottery has already been drawn for gift {giftId}");
            }

            var gift = _giftRepository.GetById(giftId);
            if (gift == null)
            {
                throw new ArgumentException($"Gift with ID {giftId} not found");
            }

            var purchases = _purchaseRepository.GetPurchasesByGiftId(giftId).Where(p => p.Status == "paid").ToList();
            if (!purchases.Any())
            {
                throw new InvalidOperationException($"No paid purchases found for gift {giftId}");
            }

            // Create ticket list based on quantity
            var tickets = new List<int>();
            foreach (var purchase in purchases)
            {
                for (int i = 0; i < purchase.Quantity; i++)
                {
                    tickets.Add(purchase.UserId);
                }
            }

            if (!tickets.Any())
            {
                throw new InvalidOperationException($"No tickets found for gift {giftId}");
            }

            // Draw winner using cryptographically secure random
            var winnerUserId = tickets[SecureRandom.NextUnbiased(tickets.Count)];
            var winner = _userRepository.GetById(winnerUserId);
            if (winner == null)
            {
                throw new InvalidOperationException($"Winner user {winnerUserId} not found");
            }

            // Create lottery result
            var lotteryResult = new LotteryResult
            {
                Gift = gift,
                GiftId = giftId,
                WinnerUserId = winnerUserId,
                Date = DateTime.UtcNow
            };

            _lotteryRepository.Add(lotteryResult);

            // Mark gift as lottery completed
            gift.IsLotteryCompleted = true;
            _giftRepository.Update(gift);

            _logger.LogInformation("Lottery drawn for gift {GiftId}, winner: {WinnerUserId}", giftId, winnerUserId);

            // Send notification email
            try
            {
                await _emailHelper.SendWinnerNotificationAsync(winner.Email, winner.Name, gift.Name, gift.Price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send winner notification email to {Email}", winner.Email);
            }
        }

        public async Task DrawLotteryAsync(int giftId)
        {
            await DrawLottery(giftId);
        }

        public bool IsLotteryCompleted()
        {
            return _systemStateService.IsLotteryOccurred();
        }

        public IEnumerable<GiftDto> GetGiftsAwaitingLottery()
        {
            // Get all gifts that have paid purchases but no lottery result
            var giftsWithPurchases = _purchaseRepository.GetAll()
                .Where(p => p.Status == "paid")
                .Select(p => p.GiftId)
                .Distinct()
                .ToList();

            var giftsWithLottery = _lotteryRepository.GetAll()
                .Select(lr => lr.GiftId)
                .Distinct()
                .ToList();

            var awaitingGiftIds = giftsWithPurchases.Except(giftsWithLottery).ToList();

            return _giftRepository.GetAll()
                .Where(g => awaitingGiftIds.Contains(g.Id))
                .Select(g => new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name ?? string.Empty,
                    CategoryColor = g.Category?.Color,
                    Price = g.Price,
                    DonorId = g.DonorId,
                    DonorName = g.Donor?.Name ?? string.Empty,
                    BuyersCount = _purchaseRepository.GetPurchasesByGiftId(g.Id).Where(p => p.Status == "paid").Sum(p => p.Quantity),
                    IsLotteryCompleted = g.IsLotteryCompleted
                })
                .ToList();
        }

        public decimal GetTotalRevenue()
        {
            return _purchaseRepository.GetAll()
                .Where(p => p.Status == "paid" && p.Gift != null)
                .Sum(p => p.Quantity * p.Gift!.Price);
        }

        public Dictionary<string, object> GetLotterySummary()
        {
            var totalGifts = _giftRepository.GetAll().Count();
            var completedLotteries = _lotteryRepository.GetAll().Count();
            var awaitingLotteries = GetGiftsAwaitingLottery().Count();
            var totalRevenue = GetTotalRevenue();

            return new Dictionary<string, object>
            {
                ["totalGifts"] = totalGifts,
                ["completedLotteries"] = completedLotteries,
                ["awaitingLotteries"] = awaitingLotteries,
                ["totalRevenue"] = totalRevenue
            };
        }

        // New methods for proper architecture
        public LotteryResultDto CreateLotteryResult(LotteryResultDto dto)
        {
            var gift = _giftRepository.GetById(dto.GiftId);
            if (gift == null)
                throw new ArgumentException($"Gift with ID {dto.GiftId} not found");

            var winner = _userRepository.GetById(dto.WinnerUserId);
            if (winner == null)
                throw new ArgumentException($"User with ID {dto.WinnerUserId} not found");

            var entity = new LotteryResult
            {
                GiftId = dto.GiftId,
                WinnerUserId = dto.WinnerUserId,
                Date = dto.DrawDate
            };

            _lotteryRepository.Add(entity);
            
            return new LotteryResultDto
            {
                Id = entity.Id,
                GiftId = entity.GiftId,
                GiftName = gift.Name,
                GiftPrice = gift.Price,
                CategoryName = gift.Category?.Name ?? "Unknown",
                WinnerUserId = entity.WinnerUserId,
                WinnerName = winner.Name,
                WinnerEmail = winner.Email,
                DrawDate = entity.Date
            };
        }

        public LotteryResultDto UpdateLotteryResult(int giftId, LotteryResultDto dto)
        {
            var entity = _lotteryRepository.GetByGiftId(giftId);
            if (entity == null)
                throw new ArgumentException($"Lottery result for gift {giftId} not found");

            var winner = _userRepository.GetById(dto.WinnerUserId);
            if (winner == null)
                throw new ArgumentException($"User with ID {dto.WinnerUserId} not found");

            entity.WinnerUserId = dto.WinnerUserId;
            entity.Date = dto.DrawDate;
            
            _lotteryRepository.Update(entity);

            return new LotteryResultDto
            {
                Id = entity.Id,
                GiftId = entity.GiftId,
                GiftName = entity.Gift?.Name ?? "Unknown",
                GiftPrice = entity.Gift?.Price ?? 0,
                CategoryName = entity.Gift?.Category?.Name ?? "Unknown",
                WinnerUserId = entity.WinnerUserId,
                WinnerName = winner.Name,
                WinnerEmail = winner.Email,
                DrawDate = entity.Date
            };
        }

        public bool DeleteLotteryResult(int giftId)
        {
            var entity = _lotteryRepository.GetByGiftId(giftId);
            if (entity == null)
                return false;

            _lotteryRepository.Delete(entity.Id);
            return true;
        }

        public Dictionary<string, object> DrawAllLotteries()
        {
            var debugInfo = new List<string>();
            var now = DateTime.UtcNow;

            var gifts = _giftRepository.GetAll().ToList();

            foreach (var gift in gifts)
            {
                var purchases = _purchaseRepository.GetPurchasesByGiftId(gift.Id).Where(p => p.Status == "paid").ToList();
                var tickets = new List<int>();
                
                foreach (var purchase in purchases)
                {
                    for (int i = 0; i < purchase.Quantity; i++)
                    {
                        tickets.Add(purchase.UserId);
                    }
                }

                debugInfo.Add($"GiftId={gift.Id}, GiftName={gift.Name}, Tickets=[{string.Join(",", tickets)}]");

                if (tickets.Count > 0)
                {
                    var rnd = new Random();
                    int winnerUserId = tickets[rnd.Next(tickets.Count)];
                    var winner = _userRepository.GetById(winnerUserId);
                    debugInfo.Add($"  WinnerUserId={winnerUserId}, ExistsInUsers={(winner != null)}");
                    
                    if (winner != null)
                    {
                        var result = new LotteryResult
                        {
                            Gift = gift,
                            GiftId = gift.Id,
                            WinnerUserId = winnerUserId,
                            Date = now
                        };
                        _lotteryRepository.Add(result);
                    }
                    else
                    {
                        debugInfo.Add($"  ERROR: WinnerUserId {winnerUserId} does not exist in Users table!");
                    }
                }
                else
                {
                    debugInfo.Add($"  No tickets for this gift.");
                }
            }
            
            _systemStateService.SetLotteryOccurred(true);
            
            return new Dictionary<string, object>
            {
                ["message"] = "Lottery completed for all gifts.",
                ["debug"] = debugInfo
            };
        }
    }
}