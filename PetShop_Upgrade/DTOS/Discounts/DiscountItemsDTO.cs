using PetShop_Upgrade.Models;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Discounts
{
    public class DiscountItemsDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string DiscountName { get; set; }
        public decimal DiscountValue { get; set; } // Giá trị Giảm giá
        public decimal? MinOrderValue { get; set; } // Giá trị thấp nhất có thể áp dụng
        public decimal? MaxDiscountAmount { get; set; } // Giá trị giảm giá cao nhất
        public int? MaxUsage { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public DiscountType DiscountType { get; set; } = DiscountType.FIXED_AMOUNT;
        public DateTime? EndDate { get; set; }
    }
}
