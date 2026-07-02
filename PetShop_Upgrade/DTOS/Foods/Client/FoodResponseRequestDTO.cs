using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.DTOS.Foods.Client
{
    public class FoodResponseRequestDTO : ProductResponseRequestDTO
    {
        public string Flavor { get; set; }
        public int WeightGram { get; set; }
        public DateTime? ExprireDate { get; set; }
        public string? AgeGroup { get; set; }
    }
}
