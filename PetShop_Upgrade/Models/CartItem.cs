namespace PetShop_Upgrade.Models
{
    public class CartItem
    {
        public CartItem() { }
        public int Id { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    }
}
