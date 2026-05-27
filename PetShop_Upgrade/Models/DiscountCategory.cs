namespace PetShop_Upgrade.Models
{
    public class DiscountCategory
    {
        public DiscountCategory() { }
        public int Id { get; set; }
        public int DiscountId { get; set; }
        public Discount Discount { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
