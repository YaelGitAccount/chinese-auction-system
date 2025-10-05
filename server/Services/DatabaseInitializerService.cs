using Microsoft.EntityFrameworkCore;
using server_NET.Data;
using server_NET.Models;
using System.Text.Json;

namespace server_NET.Services
{
    public class DatabaseInitializerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseInitializerService> _logger;

        public DatabaseInitializerService(AppDbContext context, ILogger<DatabaseInitializerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();

                // Seed categories if they don't exist
                if (!await _context.Categories.AnyAsync())
                {
                    await SeedCategoriesAsync();
                }

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        private async Task SeedCategoriesAsync()
        {
            try
            {
                var categoriesJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "categories.json");
                
                if (!File.Exists(categoriesJsonPath))
                {
                    _logger.LogWarning("Categories seed file not found at: {Path}", categoriesJsonPath);
                    return;
                }

                var jsonString = await File.ReadAllTextAsync(categoriesJsonPath);
                var categoryData = JsonSerializer.Deserialize<List<CategorySeedData>>(jsonString);

                if (categoryData == null || !categoryData.Any())
                {
                    _logger.LogWarning("No category data found in seed file");
                    return;
                }

                var categories = categoryData.Select(cd => new Category
                {
                    Name = cd.Name,
                    Description = cd.Description,
                    Color = cd.Color,
                    IsActive = cd.IsActive,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.Categories.AddRangeAsync(categories);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} categories", categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding categories");
                throw;
            }
        }

        private class CategorySeedData
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Color { get; set; }
            public bool IsActive { get; set; } = true;
        }
    }
}
