using Microsoft.EntityFrameworkCore;
using server_NET.Data;
using server_NET.Models;
using System.Collections.Generic;
using System.Linq;

namespace server_NET.Repositories
{
    public class LotteryRepository : ILotteryRepository
    {
        private readonly AppDbContext _context;
        public LotteryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<LotteryResult> GetAll()
        {
            return _context.LotteryResults
                .Include(lr => lr.WinnerUser)
                .Include(lr => lr.Gift)
                    .ThenInclude(g => g.Category)
                .ToList();
        }

        public LotteryResult? GetByGiftId(int giftId)
        {
            return _context.LotteryResults
                .Include(lr => lr.WinnerUser)
                .Include(lr => lr.Gift)
                    .ThenInclude(g => g.Category)
                .FirstOrDefault(lr => lr.Gift.Id == giftId);
        }

        public void Add(LotteryResult result)
        {
            _context.LotteryResults.Add(result);
            _context.SaveChanges();
        }

        public void Update(LotteryResult result)
        {
            _context.LotteryResults.Update(result);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.LotteryResults.FirstOrDefault(lr => lr.Id == id);
            if (entity != null)
            {
                _context.LotteryResults.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
