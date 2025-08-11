namespace API.Model.DTO
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}