namespace PetShop_Upgrade.Models
{
    public class OrderDetail
    {
        public OrderDetail() { }
        public int Id { get; set; }
        public int Quantity { get; set; }
        public double ImportPrice { get; set; }
        public double SellingPrice { get; set; }
        public double TotalPrice { get; set; }
        public int ProductColorId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
