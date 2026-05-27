namespace PetShop_Upgrade.Models
{
    public class ProductColor
    {
        public ProductColor() { }
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ColorId { get; set; }
        public Color Color { get; set; }
        public ICollection<InventoryLock> InventoryLocks { get; set; } = [];
    }
}
