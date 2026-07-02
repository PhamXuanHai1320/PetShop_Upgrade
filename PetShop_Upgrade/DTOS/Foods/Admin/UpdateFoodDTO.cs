using PetShop_Upgrade.DTOS.Products.Admin;

namespace PetShop_Upgrade.DTOS.Foods.Admin
{
    public class UpdateFoodDTO : UpdateProductDTO
    {
        public string Flavor { get; set; }
        public int WeightGram { get; set; }
        public DateTime? ExprireDate { get; set; }
        public string? AgeGroup { get; set; }
    }
}
