using Microsoft.EntityFrameworkCore;
using server_NET.Models;
using System.Collections.Generic;
using System.Linq;
using server_NET.Data;

namespace server_NET.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly AppDbContext _context;
        public GiftRepository(AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Gift> GetAll() => _context.Gifts
            .Include(g => g.Donor)
            .Include(g => g.Category)
            .Include(g => g.Purchases)
            .ToList();
        
        public Gift? GetById(int id) => _context.Gifts
            .Include(g => g.Donor)
            .Include(g => g.Category)
            .Include(g => g.Purchases)
            .FirstOrDefault(g => g.Id == id);
        public void Add(Gift gift) { _context.Gifts.Add(gift); _context.SaveChanges(); }
        public void Update(Gift gift) { _context.Gifts.Update(gift); _context.SaveChanges(); }
        public void Delete(int id)
        {
            var gift = _context.Gifts.FirstOrDefault(g => g.Id == id);
            if (gift != null) { _context.Gifts.Remove(gift); _context.SaveChanges(); }
        }
    }
}
