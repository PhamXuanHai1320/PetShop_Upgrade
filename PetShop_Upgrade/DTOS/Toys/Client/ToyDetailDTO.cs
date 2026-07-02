using PetShop_Upgrade.DTOS.Products.Client;

namespace PetShop_Upgrade.DTOS.Toys.Client
{
    public class ToyDetailDTO : ProductDetailDTO
    {
        public string Material { get; set; }
        public string Size { get; set; }
    }
}
