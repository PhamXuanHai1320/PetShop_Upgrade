using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Discounts
{
    public class DiscountDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string DiscountName { get; set; }
        public string? Description { get; set; }
        public decimal DiscountValue { get; set; } // Giá trị Giảm giá
        public decimal? MinOrderValue { get; set; } // Giá trị thấp nhất có thể áp dụng
        public decimal? MaxDiscountAmount { get; set; } // Giá trị giảm giá cao nhất
        public int? MaxUsage { get; set; } // Tổng số lần mã này được nhập
        public int? MaxUsagePerUser { get; set; } // Số lần tối đa một người dùng được phép nhập mã này
        public DiscountType DiscountType { get; set; } = DiscountType.FIXED_AMOUNT;// 0: percentage, 1: fixed amount
        public DiscountScope Scope { get; set; } = DiscountScope.PRODUCT_CATEGORY;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IsActive IsActive { get; set; } = IsActive.ACTIVE; // 0: InActive; 1: Active
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
    }
}
