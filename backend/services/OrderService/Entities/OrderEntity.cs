namespace OrderService.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public OrderStatus Status { get; set; } // 0=Created, 1=Paid, 2=Cancelled
        public DateTimeOffset CreatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Created = 0,
        Paid = 1,
        Cancelled = 2
    }
}
