using PetShop_Upgrade.DTOS.Products.Admin;

namespace PetShop_Upgrade.DTOS.Toys.Admin
{
    public class UpdateToyDTO : UpdateProductDTO
    {
        public string Material { get; set; }
        public string Size { get; set; }
    }
}
