using server_NET.Models;
using server_NET.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace server_NET.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _context;
        public PurchaseRepository(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Purchase> GetAll() => _context.Purchases.Include(p => p.User).Include(p => p.Gift).ToList();
        public Purchase? GetById(int id) => _context.Purchases.Include(p => p.User).Include(p => p.Gift).FirstOrDefault(p => p.Id == id);
        public void Add(Purchase purchase)
        {
            _context.Purchases.Add(purchase);
            _context.SaveChanges();
        }
        public void Update(Purchase purchase)
        {
            _context.Purchases.Update(purchase);
            _context.SaveChanges();
        }
        public void Delete(int id)
        {
            var p = _context.Purchases.Find(id);
            if (p != null)
            {
                _context.Purchases.Remove(p);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Purchase> GetPurchasesByGiftId(int giftId)
        {
            return _context.Purchases
                .Include(p => p.User)
                .Include(p => p.Gift)
                .Where(p => p.GiftId == giftId)
                .ToList();
        }

        // Performance optimization: Get ticket counts for all gifts in single query
        public Dictionary<int, int> GetTicketCountsByGiftIds(IEnumerable<int> giftIds)
        {
            return _context.Purchases
                .Where(p => giftIds.Contains(p.GiftId) && p.Status == "paid")
                .GroupBy(p => p.GiftId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
        }

        // Performance optimization: Get all ticket counts in single query
        public Dictionary<int, int> GetAllTicketCounts()
        {
            return _context.Purchases
                .Where(p => p.Status == "paid")
                .GroupBy(p => p.GiftId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
        }
    }
}
