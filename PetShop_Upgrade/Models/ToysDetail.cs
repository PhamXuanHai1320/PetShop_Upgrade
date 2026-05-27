namespace PetShop_Upgrade.Models
{
    public class ToysDetail
    {
        public ToysDetail() { }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Material { get; set; }
        public string Size { get; set; }
    }
}
