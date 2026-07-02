using PetShop_Upgrade.DTOS.Products.Admin;

namespace PetShop_Upgrade.DTOS.Toys.Admin
{
    public class CreateToyDTO : CreateProductDTO
    {
        public string Material { get; set; }
        public string Size { get; set; }
    }
}
