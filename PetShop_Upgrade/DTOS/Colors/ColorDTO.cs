using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Colors
{
    public class ColorDTO
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public IsActive IsActive { get; set; } = IsActive.ACTIVE; // 0: InActive; 1: Active
    }
}
