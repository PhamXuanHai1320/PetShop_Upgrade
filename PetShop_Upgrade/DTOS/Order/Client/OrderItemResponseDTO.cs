namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class OrderItemResponseDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; } 
        public string ProductColorName { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
    }
}
