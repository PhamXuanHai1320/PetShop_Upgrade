namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class OrderPreviewRequestDTO
    {
        public int DiscountId { get; set; }
        public List<CreateOrderItemRequestDTO> Items { get; set; } = [];
    }
}
