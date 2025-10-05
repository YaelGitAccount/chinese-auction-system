using server_NET.DTOs;
using System.Collections.Generic;

namespace server_NET.Services
{
    public interface IGiftService
    {
        IEnumerable<GiftDto> GetAllGifts();
        IEnumerable<GiftDto> GetAvailableGifts(); // Gifts that can still be purchased (no lottery drawn)
        GiftDto? GetGiftById(int id);
        GiftDto CreateGift(CreateGiftDto createGiftDto);
        bool UpdateGift(int id, UpdateGiftDto updateDto);
        bool DeleteGift(int id);
        bool AddGiftToDonor(int donorId, CreateGiftDto giftDto);
        string? GetWinnerNameForGift(int giftId);
    }
}
