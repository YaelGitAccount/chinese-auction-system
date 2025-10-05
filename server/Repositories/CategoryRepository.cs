using server_NET.Models;
using server_NET.Data;
using Microsoft.EntityFrameworkCore;

namespace server_NET.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetAll()
        {
            return _context.Categories
                .Include(c => c.Gifts)
                .OrderBy(c => c.Name)
                .ToList();
        }

        public Category? GetById(int id)
        {
            return _context.Categories
                .Include(c => c.Gifts)
                .FirstOrDefault(c => c.Id == id);
        }

        public void Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Category> GetActiveCategories()
        {
            return _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
        }

        public bool ExistsByName(string name, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return query.Any();
        }
    }
}
