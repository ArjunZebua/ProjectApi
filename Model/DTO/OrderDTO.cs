using API.Model.DB;

namespace API.Model.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public OrderStatus Status { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalItems { get; set; }

        public List<OrderItemDTO>? OrderItems { get; set; }
    }

    public class CreateOrderDTO
    {
        public int CustomerId { get; set; }
        public decimal ShippingCost { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public List<CreateOrderItemDTO> OrderItems { get; set; } = new();
    }

    public class CreateOrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderDTO
    {
        public int Id { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}