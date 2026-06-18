namespace PetShop_Upgrade.DTOS
{
    public class DiscountFilterDTO
    {
        public string? Keyword { get; set; } // tìm theo Code hoặc DiscountName
        public int? DiscountType { get; set; }
        public int? IsActive { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
    }
}
