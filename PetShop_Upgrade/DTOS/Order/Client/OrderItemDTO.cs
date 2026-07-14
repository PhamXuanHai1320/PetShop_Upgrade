using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductImageUrl { get; set; } = default!;
        public string ProductColorName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
