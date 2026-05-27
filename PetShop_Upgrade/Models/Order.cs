namespace PetShop_Upgrade.Models
{
    public class Order
    {
        public Order() { }
        public int Id { get; set; }
        public double TotalPrice { get; set; }
        public double DiscountPrice { get; set; }
        public double FinalPrice { get; set; }
        public string status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int AddressId { get; set; }
        public Address Address { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
        public ICollection<DiscountUsage> DiscountUsages { get; set; } = [];
        public ICollection<InventoryLock> InventoryLocks { get; set; } = [];
        public Payment Payment { get; set; }
    }
}
