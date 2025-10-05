using server_NET.DTOs;
using server_NET.Models;
using server_NET.Repositories;

namespace server_NET.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<CategoryDto> GetAll()
        {
            try
            {
                return _categoryRepository.GetAll().Select(MapToDto);
            }
            catch
            {
                return Enumerable.Empty<CategoryDto>();
            }
        }

        public IEnumerable<CategoryDto> GetActiveCategories()
        {
            try
            {
                return _categoryRepository.GetActiveCategories().Select(MapToDto);
            }
            catch
            {
                return Enumerable.Empty<CategoryDto>();
            }
        }

        public CategoryDto? GetById(int id)
        {
            if (id <= 0) return null;
            try
            {
                var category = _categoryRepository.GetById(id);
                return category != null ? MapToDto(category) : null;
            }
            catch
            {
                return null;
            }
        }

        public CategoryDto? Create(CreateCategoryDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                return null;

            try
            {
                // Check that there is no category with this name
                if (_categoryRepository.ExistsByName(createDto.Name))
                    return null;

                var category = new Category
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description?.Trim(),
                    Color = createDto.Color?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _categoryRepository.Add(category);
                return MapToDto(category);
            }
            catch
            {
                return null;
            }
        }

        public CategoryDto? Update(int id, UpdateCategoryDto updateDto)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(updateDto.Name))
                return null;

            try
            {
                var category = _categoryRepository.GetById(id);
                if (category == null)
                    return null;

                // בדיקה שאין קטגוריה אחרת עם השם הזה
                if (_categoryRepository.ExistsByName(updateDto.Name, id))
                    return null;

                category.Name = updateDto.Name.Trim();
                category.Description = updateDto.Description?.Trim();
                category.Color = updateDto.Color?.Trim();
                category.IsActive = updateDto.IsActive;

                _categoryRepository.Update(category);
                return MapToDto(category);
            }
            catch
            {
                return null;
            }
        }

        public bool Delete(int id)
        {
            if (id <= 0) return false;
            try
            {
                var category = _categoryRepository.GetById(id);
                if (category == null)
                    return false;

                // אם יש מתנות בקטגוריה, לא מוחקים
                if (category.Gifts.Any())
                    return false;

                _categoryRepository.Delete(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                GiftsCount = category.Gifts?.Count ?? 0
            };
        }
    }
}
