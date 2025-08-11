namespace API.Model.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string NamaProduct { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Harga { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Foreign Key
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        // Categories
        public List<CategoryDTO>? Categories { get; set; }
        public List<int>? CategoryIds { get; set; }

        // Reviews
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Additional properties
        public IFormFile? ImageFile { get; set; }
    }
}