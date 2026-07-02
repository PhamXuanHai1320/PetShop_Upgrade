using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.DTOS.Pets.Client
{
    public class PetDetailDTO : ProductDetailDTO
    {
        public string Gender { get; set; }
        public string Size { get; set; }
        public int Weight { get; set; }
    }
}
