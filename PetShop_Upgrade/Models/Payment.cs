using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Payment
    {
        public Payment() { }
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public CurrencyPrice Currency { get; set; } = CurrencyPrice.VND;//loại tiền tệ
        public string? Description { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreateAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}
