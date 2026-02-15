namespace PaymentService.Entities
{
    public class PaymentEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public bool Success { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
