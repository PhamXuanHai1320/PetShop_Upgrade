namespace PetShop_Upgrade.DTOS
{
    public class CreatePetDTO : CreateProductDTO
    {
        public string Gender { get; set; }
        public string Size { get; set; }
        public int Weight { get; set; }
    }
}
