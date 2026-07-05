using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Admin
{
    public class AdminProductItemDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ImportPrice { get; set; }
        public ProductType Type { get; set; }
        public IsActive IsActive { get; set; }
        public string CategoryName { get; set; }
        public string ProductImage { get; set; }
    }
}
