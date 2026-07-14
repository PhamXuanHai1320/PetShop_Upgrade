using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class CreateOrderItemRequestDTO
    {
        public int AddressId { get; set; }
        public int DiscountId { get; set; }
        public string? Note { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int ProductId { get; set; }
        public int ProductColorId { get; set; }
        public int Quantity { get; set; }
    }
}
