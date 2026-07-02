using PetShop_Upgrade.DTOS.Colors;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Admin
{
    public class CreateProductDTO
    {
        public int? Id { get; set; }
        public string ProductName { get; set; }
        public string? Description { get; set; }
        public double ImportPrice { get; set; }
        public double SellingPrice { get; set; }
        public ProductType? Type { get; set; }
        public DiscountType IsActive { get; set; } = DiscountType.FIXED_AMOUNT;
        public int CategoryId { get; set; }
        public List<IFormFile> Files { get; set; }
        public List<ProductColorRequestDTO> ProductColors { get; set; } = [];
    }
}
