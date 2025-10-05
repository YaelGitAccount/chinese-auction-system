using server_NET.DTOs;
using server_NET.Models;
using server_NET.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace server_NET.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _giftRepository;
        private readonly IDonorRepository _donorRepository;
        private readonly ILotteryRepository _lotteryRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        
        public GiftService(IGiftRepository giftRepository, IDonorRepository donorRepository, ILotteryRepository lotteryRepository, IPurchaseRepository purchaseRepository)
        {
            _giftRepository = giftRepository;
            _donorRepository = donorRepository;
            _lotteryRepository = lotteryRepository;
            _purchaseRepository = purchaseRepository;
        }

        public IEnumerable<GiftDto> GetAllGifts()
        {
            try
            {
                var gifts = _giftRepository.GetAll().ToList();
                
                // Performance optimization: Get all ticket counts in single query
                var ticketCounts = _purchaseRepository.GetAllTicketCounts();
                
                // Performance optimization: Get all winner names in single query
                var winnerNames = GetAllWinnerNames();
                
                return gifts.Select(g => new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name ?? string.Empty,
                    CategoryColor = g.Category?.Color,
                    Price = g.Price,
                    DonorId = g.DonorId,
                    DonorName = g.Donor?.Name ?? string.Empty,
                    BuyersCount = ticketCounts.GetValueOrDefault(g.Id, 0),
                    WinnerName = winnerNames.GetValueOrDefault(g.Id),
                    IsLotteryCompleted = g.IsLotteryCompleted
                });
            }
            catch
            {
                throw new Exception("Database error while fetching gifts.");
            }
        }

        public GiftDto? GetGiftById(int id)
        {
            try
            {
                var g = _giftRepository.GetById(id);
                if (g == null) return null;
                
                // Performance optimization: Get ticket count with aggregate query
                var ticketCounts = _purchaseRepository.GetTicketCountsByGiftIds(new[] { id });
                
                return new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name ?? string.Empty,
                    CategoryColor = g.Category?.Color,
                    Price = g.Price,
                    DonorId = g.DonorId,
                    DonorName = g.Donor?.Name ?? string.Empty,
                    BuyersCount = ticketCounts.GetValueOrDefault(id, 0),
                    WinnerName = GetWinnerNameForGift(g.Id),
                    IsLotteryCompleted = g.IsLotteryCompleted
                };
            }
            catch
            {
                throw new Exception("Database error while fetching gift by id.");
            }
        }

        public GiftDto CreateGift(CreateGiftDto createGiftDto)
        {
            try
            {
                var donor = _donorRepository.GetById(createGiftDto.DonorId);
                if (donor == null)
                    throw new Exception("Donor not found");
                var gift = new Gift
                {
                    Name = createGiftDto.Name,
                    CategoryId = createGiftDto.CategoryId,
                    Price = createGiftDto.Price,
                    Donor = donor,
                    DonorId = donor.Id
                };
                _giftRepository.Add(gift);
                return new GiftDto
                {
                    Id = gift.Id,
                    Name = gift.Name,
                    CategoryId = gift.CategoryId,
                    CategoryName = gift.Category?.Name ?? string.Empty,
                    CategoryColor = gift.Category?.Color,
                    Price = gift.Price,
                    DonorId = donor.Id,
                    DonorName = donor.Name,
                    BuyersCount = 0 // New gift has no purchases yet
                };
            }
            catch
            {
                throw new Exception("Database error while creating gift.");
            }
        }

        public bool UpdateGift(int id, UpdateGiftDto updateDto)
        {
            try
            {
                var gift = _giftRepository.GetById(id);
                if (gift == null) return false;
                gift.Name = updateDto.Name;
                gift.CategoryId = updateDto.CategoryId;
                gift.Price = updateDto.Price;
                if (updateDto.DonorId > 0)
                {
                    var donor = _donorRepository.GetById(updateDto.DonorId);
                    if (donor != null)
                    {
                        gift.Donor = donor;
                        gift.DonorId = donor.Id;
                    }
                }
                _giftRepository.Update(gift);
                return true;
            }
            catch
            {
                throw new Exception("Database error while updating gift.");
            }
        }

        public bool DeleteGift(int id)
        {
            try
            {
                var gift = _giftRepository.GetById(id);
                if (gift == null) return false;
                _giftRepository.Delete(id);
                return true;
            }
            catch
            {
                throw new Exception("Database error while deleting gift.");
            }
        }

        public bool AddGiftToDonor(int donorId, CreateGiftDto giftDto)
        {
            try
            {
                var donor = _donorRepository.GetById(donorId);
                if (donor == null) return false;
                var gift = new Gift
                {
                    Name = giftDto.Name,
                    CategoryId = giftDto.CategoryId,
                    Price = giftDto.Price,
                    Donor = donor
                };
                donor.Gifts.Add(gift);
                _giftRepository.Add(gift);
                _donorRepository.Update(donor);
                return true;
            }
            catch
            {
                throw new Exception("Database error while adding gift to donor.");
            }
        }

        public string? GetWinnerNameForGift(int giftId)
        {
            var result = _lotteryRepository.GetByGiftId(giftId);
            return result?.WinnerUser?.Name;
        }

        // Performance optimization: Get all winner names in single query
        private Dictionary<int, string> GetAllWinnerNames()
        {
            return _lotteryRepository.GetAll()
                .Where(lr => lr.WinnerUser != null)
                .ToDictionary(lr => lr.GiftId, lr => lr.WinnerUser!.Name);
        }

        public IEnumerable<GiftDto> GetAvailableGifts()
        {
            try
            {
                // Return only gifts without completed lottery
                var availableGifts = _giftRepository.GetAll()
                    .Where(g => !g.IsLotteryCompleted)
                    .ToList();
                
                // Performance optimization: Get ticket counts for available gifts only
                var giftIds = availableGifts.Select(g => g.Id);
                var ticketCounts = _purchaseRepository.GetTicketCountsByGiftIds(giftIds);
                
                return availableGifts.Select(g => new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name ?? string.Empty,
                    CategoryColor = g.Category?.Color,
                    Price = g.Price,
                    DonorId = g.DonorId,
                    DonorName = g.Donor?.Name ?? string.Empty,
                    BuyersCount = ticketCounts.GetValueOrDefault(g.Id, 0),
                    WinnerName = null, // No winner yet
                    IsLotteryCompleted = false
                });
            }
            catch
            {
                throw new Exception("Database error while fetching available gifts.");
            }
        }
    }
}
