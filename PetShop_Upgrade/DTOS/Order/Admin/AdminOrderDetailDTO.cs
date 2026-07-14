using PetShop_Upgrade.DTOS.Order.Client;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.DTOS.Order.Admin
{
    public class AdminOrderDetailDTO
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Note { get; set; }
        public string MemberOrderName { get; set; } = default!;
        public string MemberOrderEmail { get; set; } = default!;
        public string MemberOrderPhoneNumber { get; set; } = default!;
        public string ShippingCityName { get; set; } = default!;
        public string ShippingWardName { get; set; } = default!;
        public string ShippingPhoneNumber { get; set; } = default!;
        public string ShippingAddressDetail { get; set; } = default!;
        public string? CancelReason { get; set; }
        public int? CancelledByAdminId { get; set; }
        public string? CancelledByAdminName { get; set; }
        public DateTime? CancelledAt { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public List<OrderItemDTO> OrderDetails { get; set; } = new List<OrderItemDTO>();
    }
}
