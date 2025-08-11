using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.DB
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NamaProduct { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal Harga { get; set; }

        [Required]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Foreign Key
        public int SupplierId { get; set; }

        // Navigation Properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Additional properties that were missing
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public List<int>? CategoryIds { get; set; }
    }
}