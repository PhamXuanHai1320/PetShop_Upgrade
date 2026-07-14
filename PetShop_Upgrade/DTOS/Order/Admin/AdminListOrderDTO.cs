using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Admin
{
    public class AdminListOrderDTO
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public decimal FinalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = default!;
        public PaymentStatus PaymentStatus { get; set; } = default!;
        public int TotalItems { get; set; }
        public string MemberName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

    }
}
