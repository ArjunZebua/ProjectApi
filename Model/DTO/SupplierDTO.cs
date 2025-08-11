namespace API.Model.DTO
{
    public class SupplierDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Summary information
        public int ProductCount { get; set; }
    }
}