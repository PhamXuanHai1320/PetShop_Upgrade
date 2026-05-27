namespace PetShop_Upgrade.Models
{
    public class Color
    {
        public Color() { }
        public int Id { get; set; }
        public string ColorName { get; set; }
        public DateTime CreateAt { get; set; }
        public ICollection<ProductColor> ProductColors { get; set; } = [];
    }
}
