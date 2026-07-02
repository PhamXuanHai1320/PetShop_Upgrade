using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Admin
{
    public class AdminProductFilterDTO
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
        public double? ImportPrice { get; set; }
        public double? SellingPrice { get; set; }
        public ProductType? Type { get; set; } // 0: Pet, 1: Toy, 2: Food
    }
}
