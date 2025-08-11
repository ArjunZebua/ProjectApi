using System.ComponentModel.DataAnnotations;

namespace API.Model.DB
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
    }
}