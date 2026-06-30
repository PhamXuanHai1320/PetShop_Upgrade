namespace PetShop_Upgrade.DTOS
{
    public class FoodFillerDTO : ProductFilterDTO
    {
        public string? Flavor { get; set; }
        public int? MinWeightGram { get; set; }
        public int? MaxWeightGram { get; set; }
        public string? AgeGroup { get; set; }
    }
}
