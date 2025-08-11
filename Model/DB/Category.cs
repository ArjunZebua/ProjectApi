using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.DB
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        // [Column("Categories")] // ✅ Map property 'Name' ke kolom tabel
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        // ✅ Static property untuk seeding data awal
        public static List<Category> Categories => new List<Category>
        {
            new Category { Id = 1, Name = "Makanan", Description = "Produk makanan" },
            new Category { Id = 2, Name = "Minuman", Description = "Produk minuman" }
        };
    }
}
