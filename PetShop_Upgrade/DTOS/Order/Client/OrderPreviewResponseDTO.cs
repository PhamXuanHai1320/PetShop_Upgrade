namespace PetShop_Upgrade.DTOS.Order
{
    public class OrderPreviewResponseDTO
    {
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
