using server_NET.DTOs;

namespace server_NET.Services
{
    public interface ILotteryService
    {
        IEnumerable<LotteryResultDto> GetAllResults();
        LotteryResultDto? GetResultByGiftId(int giftId);
        Task DrawLottery(int giftId);
        Task DrawLotteryAsync(int giftId);
        bool IsLotteryCompleted();
        IEnumerable<GiftDto> GetGiftsAwaitingLottery();
        decimal GetTotalRevenue();
        Dictionary<string, object> GetLotterySummary();
        
        // Methods for proper architecture separation
        LotteryResultDto CreateLotteryResult(LotteryResultDto dto);
        LotteryResultDto UpdateLotteryResult(int giftId, LotteryResultDto dto);
        bool DeleteLotteryResult(int giftId);
        Dictionary<string, object> DrawAllLotteries();
    }
}
