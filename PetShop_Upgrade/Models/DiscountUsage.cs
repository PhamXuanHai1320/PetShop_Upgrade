namespace PetShop_Upgrade.Models
{
    public class DiscountUsage
    {
        public DiscountUsage() { }
        public int Id { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public int DiscountId { get; set; }
        public Discount Discount { get; set; }
        public int MemberId { get; set; }   
        public Member Member { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
