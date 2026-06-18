namespace PetShop_Upgrade.DTOS
{
    public class CreateDiscountDTO
    {
        public string Code { get; set; }
        public string DiscountName { get; set; }
        public string? Description { get; set; }
        public double DiscountValue { get; set; } // Giá trị Giảm giá
        public double? MinOrderValue { get; set; } // Giá trị thấp nhất có thể áp dụng
        public double? MaxDiscountAmount { get; set; } // Giá trị giảm giá cao nhất
        public int? MaxUsage { get; set; } // Tổng số lần mã này được nhập
        public int? MaxUsagePerUser { get; set; } // Số lần tối đa một người dùng được phép nhập mã này
        public int DiscountType { get; set; } = 1;// 0: percentage, 1: fixed amount
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int IsActive { get; set; } = 1; // 0: InActive; 1: Active
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }

        public List<int> ProductIds { get; set; } = [];
        public List<int> CategoryIds { get; set; } = [];
    }
}
