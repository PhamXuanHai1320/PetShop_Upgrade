namespace PetShop_Upgrade.DTOS
{
    public class UpdateToyDTO : UpdateProductDTO
    {
        public string Material { get; set; }
        public string Size { get; set; }
    }
}
