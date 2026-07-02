using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.DTOS.Pets.Client
{
    public class PetFilterDTO : ProductFilterDTO
    {
        public string? Gender { get; set; }
        public string? Size { get; set; }
        public int? MinWeight { get; set; }
        public int? MaxWeight { get; set; }
    }
}
