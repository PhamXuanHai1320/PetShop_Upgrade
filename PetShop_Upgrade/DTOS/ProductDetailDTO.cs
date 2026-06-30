using System.Text.Json.Serialization;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS
{
    [JsonDerivedType(typeof(FoodDetailDTO), typeDiscriminator: "food")]
    [JsonDerivedType(typeof(ToyDetailDTO), typeDiscriminator: "toy")]
    [JsonDerivedType(typeof(PetDetailDTO), typeDiscriminator: "pet")]
    public class ProductDetailDTO
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string? Description { get; set; }
        public double SellingPrice { get; set; }
        public double? FinalPrice { get; set; }
        public ProductType Type { get; set; }
        public IsActive IsActive { get; set; }
        public string CategoryName { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<ProductImagesResponseDTO> ProductImages { get; set; }
        public List<ProductColorResponseDTO> ProductColors { get; set; }
    }
}
