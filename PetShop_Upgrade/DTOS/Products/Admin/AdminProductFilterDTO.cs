using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Admin
{
    public class AdminProductFilterDTO
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
        public decimal? ImportPrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public ProductType? Type { get; set; } // 0: Pet, 1: Toy, 2: Food
    }
}
