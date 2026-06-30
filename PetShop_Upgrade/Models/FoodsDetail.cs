namespace PetShop_Upgrade.Models
{
    public class FoodsDetail
    {
        public FoodsDetail() { }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Flavor { get; set; } 
        public int WeightGram { get; set; }
        public DateTime? ExprireDate { get; set; }
        public string? AgeGroup { get; set; }
    }
}
