using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.DTOS.Foods.Client
{
    public class FoodFilterDTO : ProductFilterDTO
    {
        public string? Flavor { get; set; }
        public int? MinWeightGram { get; set; }
        public int? MaxWeightGram { get; set; }
        public string? AgeGroup { get; set; }
    }
}
