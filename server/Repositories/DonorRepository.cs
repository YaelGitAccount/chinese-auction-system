using server_NET.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace server_NET.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly Data.AppDbContext _context;
        public DonorRepository(Data.AppDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Donor> GetAll() => _context.Donors
            .Include(d => d.Gifts)
            .ThenInclude(g => g.Category)
            .ToList();
        
        public Donor? GetById(int id) => _context.Donors
            .Include(d => d.Gifts)
            .ThenInclude(g => g.Category)
            .FirstOrDefault(d => d.Id == id);
        public void Add(Donor donor) { _context.Donors.Add(donor); _context.SaveChanges(); }
        public void Update(Donor donor) { _context.Donors.Update(donor); _context.SaveChanges(); }
        public void Delete(int id)
        {
            var donor = _context.Donors.FirstOrDefault(d => d.Id == id);
            if (donor != null) { _context.Donors.Remove(donor); _context.SaveChanges(); }
        }
    }
}
