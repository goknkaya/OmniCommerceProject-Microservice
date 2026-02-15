namespace CatalogService.Entities
{
    public class ReceivedOrderEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
    }
}
