namespace PetShop_Upgrade.DTOS.Cart
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Price * Quantity;

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductImageUrl { get; set; }

        public int ProductColorId { get; set; }
        public string ColorName { get; set; }
    }
}
