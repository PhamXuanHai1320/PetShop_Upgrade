namespace PetShop_Upgrade.Models
{
    public class Payment
    {
        public Payment() { }
        public int Id { get; set; }
        public double Totalprice { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public DateTime PainAt { get; set; }
        public string OrderCode { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string Currency { get; set; } //loại tiền tệ
        public string Description { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public DateTime CancelledAt { get; set; }
        public DateTime CreateAt { get; set; }
        public string CancelationReason { get; set; }
    }
}
