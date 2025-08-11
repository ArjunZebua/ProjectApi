using System.ComponentModel;

namespace API.Model.DTO
{
    public class CategoryDTO
    {
        [DefaultValue(0)]
        public int Id { get; set; }

        [DefaultValue("Electronics")]
        public string Name { get; set; } = string.Empty;

        [DefaultValue("Category for electronic products and gadgets")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Additional properties for controller usage
        [DefaultValue(0)]
        public int ProductCount { get; set; }

        public List<ProductDTO>? Products { get; set; }
    }
}