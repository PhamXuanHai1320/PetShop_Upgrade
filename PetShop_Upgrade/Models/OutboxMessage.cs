namespace PetShop_Upgrade.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
        public int RetryCount { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public string? LastError { get; set; }
    }
}
