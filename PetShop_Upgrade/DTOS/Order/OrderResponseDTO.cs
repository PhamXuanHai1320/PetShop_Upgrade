using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }
        public decimal FinalPrice { get; set; }
        public string Note { get; set; }
        public string ShippingCityName { get; set; } = default!;
        public string ShippingWardName { get; set; } = default!;
        public string ShippingPhoneNumber { get; set; } = default!;
        public string ShippingAddressDetail { get; set; } = default!;
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponseDTO> Items { get; set; }
    }
}
