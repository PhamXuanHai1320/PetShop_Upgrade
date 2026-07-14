using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Admin
{
    public class AdminOrderFilterDTO
    {
        public OrderStatus? Status { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }
}
