namespace PetShop_Upgrade.DTOS
{
    public class FoodResponseRequestDTO : ProductResponseRequestDTO
    {
        public string Flavor { get; set; }
        public int WeightGram { get; set; }
        public DateTime? ExprireDate { get; set; }
        public string? AgeGroup { get; set; }
    }
}
