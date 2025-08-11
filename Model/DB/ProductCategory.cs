using System.ComponentModel.DataAnnotations;

namespace API.Model.DB
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Category Category { get; set; }
    }
}