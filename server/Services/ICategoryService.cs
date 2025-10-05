using server_NET.DTOs;

namespace server_NET.Services
{
    public interface ICategoryService
    {
        IEnumerable<CategoryDto> GetAll();
        IEnumerable<CategoryDto> GetActiveCategories();
        CategoryDto? GetById(int id);
        CategoryDto? Create(CreateCategoryDto createDto);
        CategoryDto? Update(int id, UpdateCategoryDto updateDto);
        bool Delete(int id);
    }
}
