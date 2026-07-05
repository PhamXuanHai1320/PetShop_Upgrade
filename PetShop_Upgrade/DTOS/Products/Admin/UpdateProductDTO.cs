using PetShop_Upgrade.DTOS.Colors;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Products.Admin
{
    public class UpdateProductDTO
    {
        public string ProductName { get; set; }
        public string? Description { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int MainImageIndex { get; set; } = 0;
        public int CategoryId { get; set; }
        public IsActive IsActive { get; set; }
        public int MemberId { get; set; }

        // Ảnh: phải khai báo rõ 3 nhóm
        public List<int> DeletedImageIds { get; set; } = [];      // ảnh cũ muốn xóa
        public List<IFormFile> NewImages { get; set; } = [];      // ảnh mới muốn thêm
        public List<ProductColorRequestDTO> ProductColors { get; set; } = [];
    }
}
