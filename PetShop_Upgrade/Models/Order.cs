using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Order
    {
        public Order() { }
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public string Note { get; set; }
        public string ShippingCityName { get; set; } = default!;
        public string ShippingCityCode { get; set; } = default!;
        public string ShippingWardName { get; set; } = default!;
        public string ShippingWardCode { get; set; } = default!;
        public string ShippingPhoneNumber { get; set; } = default!;
        public string ShippingAddressDetail { get; set; } = default!;
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int? CancelledByAdminId { get; set; }
        public OrderStatus status { get; set; } = OrderStatus.PENDING;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int? AddressId { get; set; }
        public Address? Address { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
        public ICollection<DiscountUsage> DiscountUsages { get; set; } = [];
        public ICollection<InventoryLock> InventoryLocks { get; set; } = [];
        public Payment Payment { get; set; }
    }
}
