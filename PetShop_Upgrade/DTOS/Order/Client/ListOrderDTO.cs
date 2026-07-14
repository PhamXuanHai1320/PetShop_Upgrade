using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class ListOrderDTO
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = default!;
        public PaymentStatus PaymentStatus { get; set; } = default!;
        public int TotalItems { get; set; }
    }
}
