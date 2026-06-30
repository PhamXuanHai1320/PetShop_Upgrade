namespace PetShop_Upgrade.DTOS
{
    public class PetFillerDTO : ProductFilterDTO
    {
        public string? Gender { get; set; }
        public string? Size { get; set; }
        public int? MinWeight { get; set; }
        public int? MaxWeight { get; set; }
    }
}
