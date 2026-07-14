using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class CreateOrderFromCartRequestDTO
    {
        public int AddressId { get; set; }
        public int DiscountId { get; set; }
        public string? Note { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<int> CartItemIds { get; set; } = [];
    }
}
