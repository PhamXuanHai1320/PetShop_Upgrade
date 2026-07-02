using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Members.Client
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProductType ProductType { get; set; }
        public IsActive IsActive { get; set; } = IsActive.ACTIVE;
    }
}
