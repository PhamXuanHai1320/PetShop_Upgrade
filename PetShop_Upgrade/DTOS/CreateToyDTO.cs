namespace PetShop_Upgrade.DTOS
{
    public class CreateToyDTO : CreateProductDTO
    {
        public string Material { get; set; }
        public string Size { get; set; }
    }
}
