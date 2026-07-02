using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Client
{
    public class ProductImageRequestDTO
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public IsMain IsMain { get; set; } = IsMain.NOT_MAIN;
        public int? ProductId { get; set; }
    }
}
