using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class InventoryLock
    {
        public InventoryLock() { }
        public int Id { get; set; }
        public int Quantity { get; set; }
        public InventoryLockStatus Status { get; set; } // "LOCK", "REBASE"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpireAt { get; set; } = DateTime.Now.AddMinutes(10);
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductColorId { get; set; }
        public ProductColor ProductColor { get; set; }
    }
}
