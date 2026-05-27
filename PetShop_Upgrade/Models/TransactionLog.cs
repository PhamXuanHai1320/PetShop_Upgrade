namespace PetShop_Upgrade.Models
{
    public class TransactionLog
    {
        public TransactionLog() { }
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string API { get; set; }
        public string Status { get; set; }
        public string Data { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
