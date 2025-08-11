using API.Model;
using API.Model.DB;
using API.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int id);
        Task<bool> AddCategoryAsync(CategoryDTO categoryDTO);
        Task<bool> UpdateCategoryAsync(int id, CategoryDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(int id);

        // Legacy sync methods
        List<CategoryDTO> GetAllCategories();
        CategoryDTO GetCategoryById(int id);
        bool AddCategory(CategoryDTO categoryDTO);
        bool UpdateCategory(int id, CategoryDTO categoryDTO);
        bool DeleteCategory(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ApplicationContext _context;

        public CategoryService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name, // ✅ FIXED: Both use 'Name' property now
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name, // ✅ FIXED: Both use 'Name' property now
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<bool> AddCategoryAsync(CategoryDTO categoryDTO)
        {
            try
            {
                var category = new Category
                {
                    Name = categoryDTO.Name, // ✅ FIXED: Both use 'Name' property now
                    Description = categoryDTO.Description,
                    CreatedAt = DateTime.UtcNow, // ✅ Fixed: UTC instead of local time
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(int id, CategoryDTO categoryDTO)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return false;

                category.Name = categoryDTO.Name; // ✅ FIXED: Both use 'Name' property now
                category.Description = categoryDTO.Description;
                category.UpdatedAt = DateTime.UtcNow; // ✅ Fixed: UTC instead of local time

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Legacy sync methods
        public List<CategoryDTO> GetAllCategories() => GetAllCategoriesAsync().Result;
        public CategoryDTO GetCategoryById(int id) => GetCategoryByIdAsync(id).Result;
        public bool AddCategory(CategoryDTO categoryDTO) => AddCategoryAsync(categoryDTO).Result;
        public bool UpdateCategory(int id, CategoryDTO categoryDTO) => UpdateCategoryAsync(id, categoryDTO).Result;
        public bool DeleteCategory(int id) => DeleteCategoryAsync(id).Result;
    }
}