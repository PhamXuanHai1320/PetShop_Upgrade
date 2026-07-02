using PetShop_Upgrade.DTOS.Products.Admin;

namespace PetShop_Upgrade.DTOS.Pets.Admin
{
    public class CreatePetDTO : CreateProductDTO
    {
        public string Gender { get; set; }
        public string Size { get; set; }
        public int Weight { get; set; }
    }
}
