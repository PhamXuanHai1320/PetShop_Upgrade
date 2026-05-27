namespace PetShop_Upgrade.Models
{
    public class Product
    {
        public Product() { }
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public double ImportPrice { get; set; }
        public double SellingPrice { get; set; }
        public int Type { get; set; }
        public int IsActive { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = [];
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
        public ICollection<ProductImage> ProductImages { get; set; } = [];
        public ICollection<ProductHistory> ProductHistories { get; set; } = [];
        public ToysDetail ToysDetails { get; set; }
        public FoodsDetail FoodsDetails { get; set; }
        public ICollection<PetVariant> PetVariants { get; set; } = [];
        public ICollection<ProductColor> ProductColors { get; set; } = [];
        public ICollection<Rating> Ratings { get; set; } = [];
        public ICollection<DiscountProduct> DiscountProducts { get; set; } = [];
    }
}
