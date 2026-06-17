namespace PetShop_Upgrade.Models
{
    public class Category
    {
        public Category() { }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductType { get; set; }
        public int IsActive { get; set; } = 1; // 0: InActive; 1: Active
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Product> Products { get; set; } = [];
        public ICollection<DiscountCategory> DiscountCategories { get; set; } = [];
    }
}
