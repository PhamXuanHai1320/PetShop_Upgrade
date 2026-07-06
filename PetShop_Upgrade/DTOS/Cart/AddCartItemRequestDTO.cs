namespace PetShop_Upgrade.DTOS.Cart
{
    public class AddCartItemRequestDTO
    {
        public int ProductId { get; set; }
        public int ProductColorId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
