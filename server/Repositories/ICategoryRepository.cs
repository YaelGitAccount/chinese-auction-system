using server_NET.Models;

namespace server_NET.Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category? GetById(int id);
        void Add(Category category);
        void Update(Category category);
        void Delete(int id);
        IEnumerable<Category> GetActiveCategories();
        bool ExistsByName(string name, int? excludeId = null);
    }
}
