using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Product
    {
        public Product() { }
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string? Description { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public ProductType Type { get; set; } // 0: Pet, 1: Toy, 2: Food
        public IsActive IsActive { get; set; } = IsActive.ACTIVE; 
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = [];
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
        public ICollection<ProductImage> ProductImages { get; set; } = [];
        public ICollection<ProductHistory> ProductHistories { get; set; } = [];
        public ToysDetail? ToysDetails { get; set; }
        public FoodsDetail? FoodsDetails { get; set; }
        public PetVariant? PetVariant { get; set; }
        public ICollection<ProductColor> ProductColors { get; set; } = [];
        public ICollection<Rating> Ratings { get; set; } = [];
        public ICollection<DiscountProduct> DiscountProducts { get; set; } = [];
        public ICollection<PetViewingAppointment> PetViewingAppointments { get; set; } = [];
    }
}
