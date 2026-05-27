namespace PetShop_Upgrade.Models
{
    public class PaymentWebhookLog
    {
        public PaymentWebhookLog() { }
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string RawPayload { get; set; }
        public string Signature { get; set; }
        public int IsVerified { get; set; }
        public DateTime ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
