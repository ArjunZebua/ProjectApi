using API.Model.DTO;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public CategoryController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }


        // GET: api/Category
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _categoryService.GetAllCategories();

            // Tambahkan jumlah produk per kategori (ProductCount)
            foreach (var category in categories)
            {
                category.ProductCount = _productService.CountProductsByCategoryId(category.Id);
            }

            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var category = _categoryService.GetCategoryById(id);
            if (category == null)
                return NotFound("Kategori tidak ditemukan.");

            // Tambahkan data produk jika ada
            category.Products = _productService.GetProductsByCategoryId(id);
            category.ProductCount = category.Products?.Count ?? 0;

            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        public IActionResult Create([FromBody] CategoryDTO categoryDto)
        {
            categoryDto.CreatedAt = DateTime.UtcNow;
            categoryDto.UpdatedAt = DateTime.UtcNow;

            _categoryService.AddCategory(categoryDto);
            return Ok("Kategori berhasil ditambahkan.");
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CategoryDTO categoryDto)
        {
            var existing = _categoryService.GetCategoryById(id);
            if (existing == null)
                return NotFound("Kategori tidak ditemukan.");

            categoryDto.UpdatedAt = DateTime.UtcNow;
            _categoryService.UpdateCategory(id, categoryDto);
            return Ok("Kategori berhasil diperbarui.");
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _categoryService.DeleteCategory(id);
            if (!result)
                return BadRequest("Gagal menghapus kategori.");

            return Ok("Kategori berhasil dihapus.");
        }
    }
}
