using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS
{
    public class AdminFoodFillerDTO : AdminProductFillerDTO
    {
        public ExpiryStatus? ExpiryStatus { get; set; }
    }
}
