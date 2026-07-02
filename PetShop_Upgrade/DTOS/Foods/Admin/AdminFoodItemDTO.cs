using PetShop_Upgrade.DTOS.Products.Admin;

namespace PetShop_Upgrade.DTOS.Foods.Admin
{
    public class AdminFoodItemDTO : AdminProductItemDTO
    {
        public DateTime? ExprireDate { get; set; }
    }
}
