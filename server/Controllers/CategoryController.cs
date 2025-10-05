using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server_NET.DTOs;
using server_NET.Services;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/Category/all - Manager only
        [Authorize(Roles = "manager")]
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all categories (including inactive)");
                var categories = _categoryService.GetAll();
                _logger.LogInformation("Successfully retrieved {Count} categories", categories.Count());
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/Category - Active categories only
        [HttpGet]
        public IActionResult GetActiveCategories()
        {
            try
            {
                var categories = _categoryService.GetActiveCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // קבלת קטגוריה לפי ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid category ID.");

                var category = _categoryService.GetById(id);
                if (category == null)
                    return NotFound("Category not found.");

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // יצירת קטגוריה חדשה - למנהל בלבד
        [Authorize(Roles = "manager")]
        [HttpPost]
        public IActionResult Create([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.Name))
                    return BadRequest("Category name is required.");

                var category = _categoryService.Create(createDto);
                if (category == null)
                    return BadRequest("Category with this name already exists or creation failed.");

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // עדכון קטגוריה - למנהל בלבד
        [Authorize(Roles = "manager")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid category ID.");

                if (string.IsNullOrWhiteSpace(updateDto.Name))
                    return BadRequest("Category name is required.");

                var category = _categoryService.Update(id, updateDto);
                if (category == null)
                    return BadRequest("Category not found or name already exists.");

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // מחיקת קטגוריה - למנהל בלבד
        [Authorize(Roles = "manager")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid category ID.");

                var result = _categoryService.Delete(id);
                if (!result)
                    return BadRequest("Category not found or has gifts associated with it.");

                return Ok("Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
