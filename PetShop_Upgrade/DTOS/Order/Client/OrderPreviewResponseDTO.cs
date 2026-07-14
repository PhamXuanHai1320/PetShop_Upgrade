namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class OrderPreviewResponseDTO
    {
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
