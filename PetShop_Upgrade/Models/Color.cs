namespace PetShop_Upgrade.Models
{
    public class Color
    {
        public Color() { }
        public int Id { get; set; }
        public string ColorName { get; set; }
        public int IsActive { get; set; } = 1; // 0: InActive; 1: Active
        public DateTime CreateAt { get; set; }
        public ICollection<ProductColor> ProductColors { get; set; } = [];
    }
}
