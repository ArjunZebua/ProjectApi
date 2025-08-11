using API.Model;
using API.Model.DB;
using API.Model.DTO;
using API.Services;
// using API.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IOrderService orderService,
            IWebHostEnvironment hostingEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                if (products == null || !products.Any())
                    return NotFound("Tidak ada produk yang tersedia.");

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Terjadi kesalahan: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound("Produk tidak ditemukan.");

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileName = request.ImageFile.FileName;
                var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                var contentType = request.ImageFile.ContentType;

                // Debug info - HAPUS SETELAH TESTING
                Console.WriteLine($"Debug - FileName: '{fileName}'");
                Console.WriteLine($"Debug - Extension: '{extension}'");
                Console.WriteLine($"Debug - ContentType: '{contentType}'");
                Console.WriteLine($"Debug - FileSize: {request.ImageFile.Length} bytes");

                // Alternative validation - check ContentType also
                var validContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };

                if ((string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension)) &&
                    !validContentTypes.Contains(contentType?.ToLowerInvariant()))
                {
                    return BadRequest($"File tidak valid. FileName: '{fileName}', Extension: '{extension}', ContentType: '{contentType}'");
                }

                if (request.ImageFile.Length > 5 * 1024 * 1024)
                    return BadRequest("Ukuran gambar maksimal 5MB.");

                // PERBAIKAN UTAMA - TAMBAH NULL CHECK
                var webRootPath = _hostingEnvironment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + "_" + request.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }

                request.ImageUrl = "/uploads/" + uniqueFileName;
            }
            else
            {
                request.ImageUrl ??= "/images/default-product.png";
            }

            var dto = new ProductDTO
            {
                NamaProduct = request.NamaProduct,
                Description = request.Description,
                Harga = request.Harga,
                Stock = request.Stock,
                ImageUrl = request.ImageUrl,
                IsActive = request.IsActive,
                Date = request.Date,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SupplierId = request.SupplierId,
                CategoryIds = request.CategoryIds,
            };

            var result = await _productService.AddProductAsync(dto);
            if (result)
                return Ok("Produk berhasil ditambahkan.");
            else
                return BadRequest("Gagal menambahkan produk.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateRequest request)
        {
            var existingProduct = await _productService.GetByIdAsync(id);
            if (existingProduct == null)
                return NotFound("Produk tidak ditemukan.");

            // Handle image upload jika ada file baru
            string imageUrl = existingProduct.ImageUrl; // Keep existing image by default

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(request.ImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Hanya file gambar yang diizinkan.");

                if (request.ImageFile.Length > 5 * 1024 * 1024)
                    return BadRequest("Ukuran gambar maksimal 5MB.");

                var webRootPath = _hostingEnvironment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Hapus gambar lama jika ada
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl) &&
                    existingProduct.ImageUrl.StartsWith("/uploads/"))
                {
                    var oldImagePath = Path.Combine(webRootPath, existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid() + "_" + request.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }

                imageUrl = "/uploads/" + uniqueFileName;
            }

            var dto = new ProductDTO
            {
                Id = id,
                NamaProduct = request.NamaProduct ?? existingProduct.NamaProduct,
                Description = request.Description ?? existingProduct.Description,
                Harga = request.Harga ?? existingProduct.Harga,
                Stock = request.Stock ?? existingProduct.Stock,
                ImageUrl = imageUrl,
                IsActive = request.IsActive ?? existingProduct.IsActive,
                Date = request.Date ?? existingProduct.Date,
                CreatedAt = existingProduct.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                SupplierId = request.SupplierId ?? existingProduct.SupplierId,
                CategoryIds = request.CategoryIds ?? existingProduct.CategoryIds,
            };

            try
            {
                Console.WriteLine($"Debug - Updating product ID: {id}");
                Console.WriteLine($"Debug - DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");

                var result = await _productService.UpdateProductAsync(id, dto);

                Console.WriteLine($"Debug - Update result: {result}");

                if (result)
                    return Ok("Produk berhasil diperbarui.");
                else
                    return BadRequest("Gagal memperbarui produk. Periksa log untuk detail.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug - Exception: {ex.Message}");
                Console.WriteLine($"Debug - StackTrace: {ex.StackTrace}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return BadRequest("Gagal menghapus produk.");

            return Ok("Produk berhasil dihapus.");
        }

        [HttpPost("checkout")]
        // [RequireLogin]
        public IActionResult Checkout(int productId, int quantity)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Unauthorized("Anda harus login.");

                var orderId = _orderService.CreateOrder(userId, productId, quantity);

                return Ok(new { message = "Checkout berhasil", orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Terjadi kesalahan saat checkout: " + ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            return 1; // Simulasi user login
        }
    }

    public class ProductCreateRequest
    {
        public string NamaProduct { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Harga { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Date { get; set; }
        public int SupplierId { get; set; }
        public List<int>? CategoryIds { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class ProductUpdateRequest
    {
        public string? NamaProduct { get; set; }
        public string? Description { get; set; }
        public decimal? Harga { get; set; }
        public int? Stock { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? Date { get; set; }
        public int? SupplierId { get; set; }
        public List<int>? CategoryIds { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}