namespace PetShop_Upgrade.Models
{
    public class Discount
    {
        public Discount() { }
        public int Id { get; set; }
        public string Code { get; set; }
        public string DiscountName { get; set; }
        public string Description { get; set; }
        public double DiscountValue { get; set; }
        public double MinOrderValue { get; set; }
        public double MaxOrderValue { get; set; }
        public int MaxUsage { get; set; }
        public int MaxUsagePerUser { get; set; }
        public int DiscountType { get; set; } = 1;// 0: percentage, 1: fixed amount
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int IsActive { get; set; } = 1; // 0: InActive; 1: Active
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public Member Member { get; set; }
        public ICollection<DiscountUsage> DiscountUsages { get; set; } = [];
        public ICollection<DiscountCategory> DiscountCategories { get; set; } = [];
        public ICollection<DiscountProduct> DiscountProducts { get; set; } = [];
    }
}
