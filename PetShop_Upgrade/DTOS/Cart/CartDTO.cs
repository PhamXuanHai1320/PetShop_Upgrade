namespace PetShop_Upgrade.DTOS.Cart
{
    public class CartDTO
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = [];
    }
}
