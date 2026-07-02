using PetShop_Upgrade.DTOS.Products.Admin;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Foods.Admin
{
    public class AdminFoodFilterDTO : AdminProductFilterDTO
    {
        public ExpiryStatus? ExpiryStatus { get; set; }
    }
}
