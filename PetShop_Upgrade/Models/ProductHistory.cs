namespace PetShop_Upgrade.Models
{
    public class ProductHistory
    {
        public ProductHistory() { }
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int Version { get; set; }
        public DateTime ChangeAt { get; set; } = DateTime.Now;
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
