using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Client
{
    public class ProductItemsDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public ProductType Type { get; set; }
        public IsActive IsActive { get; set; }
        public string CategoryName { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public string ProductImage { get; set; }
    }
}
