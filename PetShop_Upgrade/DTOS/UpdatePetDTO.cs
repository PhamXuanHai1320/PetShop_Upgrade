namespace PetShop_Upgrade.DTOS
{
    public class UpdatePetDTO : UpdateProductDTO
    {
        public string Gender { get; set; }
        public string Size { get; set; }
        public int Weight { get; set; }
    }
}
