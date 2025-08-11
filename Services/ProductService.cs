// Services/ProductService.cs - FIXED with correct property mappings
using API.Model;
using API.Model.DB;
using API.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface IProductService
    {
        Task<bool> AddProductAsync(ProductDTO productDTO);
        Task<bool> UpdateProductAsync(int id, ProductDTO productDTO);
        Task<List<ProductDTO>> GetAllAsync(bool includeCategories = true, bool includeReviews = false);
        Task<ProductDTO?> GetByIdAsync(int id, bool includeCategories = true, bool includeReviews = false);
        Task<bool> DeleteProductAsync(int id);
        Task<List<ProductDTO>> GetProductsByCategoryAsync(int categoryId);
        Task<List<ProductDTO>> SearchProductsAsync(string searchTerm);

        // Legacy sync methods
        bool AddProduct(ProductDTO product);
        bool UpdateProduct(int id, ProductDTO productDTO);
        List<ProductDTO> GetAll();
        ProductDTO GetById(int id);
        bool Delete(int id);

        // Missing methods that were causing errors
        List<ProductDTO> GetProductsByCategoryId(int categoryId);
        int CountProductsByCategoryId(int categoryId);
        bool Create(ProductDTO productDTO);
        bool Update(ProductDTO productDTO);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationContext _context;

        public ProductService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> AddProductAsync(ProductDTO productDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = new Product
                {
                    NamaProduct = productDTO.NamaProduct,
                    Description = productDTO.Description,
                    Harga = productDTO.Harga,
                    Stock = productDTO.Stock,
                    ImageUrl = productDTO.ImageUrl,
                    IsActive = productDTO.IsActive,
                    SupplierId = productDTO.SupplierId,
                    Date = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                if (productDTO.CategoryIds != null && productDTO.CategoryIds.Any())
                {
                    foreach (var categoryId in productDTO.CategoryIds)
                    {
                        var productCategory = new ProductCategory
                        {
                            ProductId = product.Id,
                            CategoryId = categoryId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.ProductCategories.AddAsync(productCategory);
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(int id, ProductDTO productDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.ProductCategories)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) return false;

                product.NamaProduct = productDTO.NamaProduct;
                product.Description = productDTO.Description;
                product.Harga = productDTO.Harga;
                product.Stock = productDTO.Stock;
                product.ImageUrl = productDTO.ImageUrl;
                product.IsActive = productDTO.IsActive;
                product.SupplierId = productDTO.SupplierId;
                product.Date = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                if (productDTO.CategoryIds != null)
                {
                    _context.ProductCategories.RemoveRange(product.ProductCategories);

                    foreach (var categoryId in productDTO.CategoryIds)
                    {
                        var productCategory = new ProductCategory
                        {
                            ProductId = product.Id,
                            CategoryId = categoryId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.ProductCategories.AddAsync(productCategory);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<ProductDTO>> GetAllAsync(bool includeCategories = true, bool includeReviews = false)
        {
            var query = _context.Products
                .Include(p => p.Supplier)
                .AsQueryable();

            if (includeCategories)
            {
                query = query.Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category);
            }

            if (includeReviews)
            {
                query = query.Include(p => p.Reviews.Where(r => r.IsApproved));
            }

            return await query
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    NamaProduct = p.NamaProduct,
                    Description = p.Description,
                    Harga = p.Harga,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    Date = p.Date,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,
                    Categories = includeCategories ? p.ProductCategories.Select(pc => new CategoryDTO
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name, // ✅ FIXED: Category.Name -> CategoryDTO.Name
                        Description = pc.Category.Description
                    }).ToList() : null,
                    CategoryIds = includeCategories ? p.ProductCategories.Select(pc => pc.CategoryId).ToList() : null,
                    AverageRating = includeReviews && p.Reviews.Any(r => r.IsApproved) ? p.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0,
                    ReviewCount = includeReviews ? p.Reviews.Count(r => r.IsApproved) : 0
                })
                .OrderBy(p => p.NamaProduct)
                .ToListAsync();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id, bool includeCategories = true, bool includeReviews = false)
        {
            var query = _context.Products
                .Include(p => p.Supplier)
                .AsQueryable();

            if (includeCategories)
            {
                query = query.Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category);
            }

            if (includeReviews)
            {
                query = query.Include(p => p.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.Customer);
            }

            var product = await query.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                NamaProduct = product.NamaProduct,
                Description = product.Description,
                Harga = product.Harga,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                Date = product.Date,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier.CompanyName,
                Categories = includeCategories ? product.ProductCategories.Select(pc => new CategoryDTO
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name, // ✅ FIXED: Category.Name -> CategoryDTO.Name
                    Description = pc.Category.Description
                }).ToList() : null,
                CategoryIds = includeCategories ? product.ProductCategories.Select(pc => pc.CategoryId).ToList() : null,
                AverageRating = includeReviews && product.Reviews.Any(r => r.IsApproved) ? product.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0,
                ReviewCount = includeReviews ? product.Reviews.Count(r => r.IsApproved) : 0
            };
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<ProductDTO>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId) && p.IsActive)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    NamaProduct = p.NamaProduct,
                    Description = p.Description,
                    Harga = p.Harga,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    Date = p.Date,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,
                    Categories = p.ProductCategories.Select(pc => new CategoryDTO
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name, // ✅ FIXED: Category.Name -> CategoryDTO.Name
                        Description = pc.Category.Description
                    }).ToList(),
                    CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<ProductDTO>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.IsActive && (
                    p.NamaProduct.Contains(searchTerm) ||
                    p.Description!.Contains(searchTerm) ||
                    p.ProductCategories.Any(pc => pc.Category.Name.Contains(searchTerm)) // ✅ FIXED: Category.Name
                ))
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    NamaProduct = p.NamaProduct,
                    Description = p.Description,
                    Harga = p.Harga,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    Date = p.Date,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.CompanyName,
                    Categories = p.ProductCategories.Select(pc => new CategoryDTO
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name, // ✅ FIXED: Category.Name -> CategoryDTO.Name
                        Description = pc.Category.Description
                    }).ToList()
                })
                .ToListAsync();
        }

        // Legacy sync methods (for backward compatibility)
        public bool AddProduct(ProductDTO product) => AddProductAsync(product).Result;
        public bool UpdateProduct(int id, ProductDTO productDTO) => UpdateProductAsync(id, productDTO).Result;
        public List<ProductDTO> GetAll() => GetAllAsync().Result;
        public ProductDTO GetById(int id) => GetByIdAsync(id).Result;
        public bool Delete(int id) => DeleteProductAsync(id).Result;

        // Missing methods that were causing errors
        public List<ProductDTO> GetProductsByCategoryId(int categoryId) => GetProductsByCategoryAsync(categoryId).Result;
        public int CountProductsByCategoryId(int categoryId)
        {
            return _context.Products
                .Count(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId) && p.IsActive);
        }

        public bool Create(ProductDTO productDTO) => AddProductAsync(productDTO).Result;
        public bool Update(ProductDTO productDTO) => UpdateProductAsync(productDTO.Id, productDTO).Result;
    }
}