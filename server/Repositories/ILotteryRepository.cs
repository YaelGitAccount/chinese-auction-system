using server_NET.Models;
using System.Collections.Generic;

namespace server_NET.Repositories
{
    public interface ILotteryRepository
    {
        IEnumerable<LotteryResult> GetAll();
        LotteryResult? GetByGiftId(int giftId);
        void Add(LotteryResult result);
        void Update(LotteryResult result);
        void Delete(int id);
    }
}
