using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS
{
    public class ProductFilterDTO
    {
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public ProductType? Type { get; set; } // 0: Pet, 1: Toy, 2: Food
        public int? ColorId { get; set; }
    }
}
