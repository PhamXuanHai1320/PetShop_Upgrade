namespace PetShop_Upgrade.Models
{
    public class Enum
    {
        public enum ProductType
        {
            Pet = 0,
            Toy = 1,
            Food = 2
        }
        public enum AppointmentStatus
        {
            PENDING = 0,
            CONFIRMED = 1,
            COMPLETED = 2,
            CANCELLED = 3
        }
        public enum OrderStatus
        {
            PENDING = 0,
            CONFIRMED = 1,
            SHIPPED = 2,
            DELIVERED = 3,
            CANCELLED = 4
        }
        public enum IsActive
        {
            INACTIVE = 0,
            ACTIVE = 1
        }
        public enum DiscountType
        {
            PERCENTAGE = 0,
            FIXED_AMOUNT = 1
        }
        public enum InventoryLockStatus
        {
            LOCKED = 0,
            REBASE = 1,
            CONFIRMED = 2
        }
        public enum IsMain
        {
            MAIN = 1,
            NOT_MAIN = 0
        }
        public enum ExpiryStatus
        {
            EXPIRED = 0,
            EXPIREDSOON = 1
        }
        public enum ServiceType
        {
            PET_VIEWING = 0
        }
        public enum DiscountScope
        {
            PRODUCT_CATEGORY = 0, // áp cho sản phẩm/danh mục cụ thể
            ORDER = 1             // áp cho tổng giá trị đơn hàng
        }
        public enum PaymentMethod
        {
            CASH = 0,
            VNPAY = 1
        }
        public enum PaymentStatus
        {
            PENDING = 0,
            PAID = 1,
            FAILED = 2
        }
        public enum CurrencyPrice
        {
            VND = 0
        }
    }
}
