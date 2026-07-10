using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories
{
    public class InventoryLockRepository : Repository<InventoryLock>, Interfaces.IInventoryLockRepository
    {
        public InventoryLockRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<List<int>> GetExpiredPendingOrderIdsAsync(DateTime utcNow)
        {
            return _context.InventoryLocks
                .AsNoTracking()
                .Where(x => x.Status == InventoryLockStatus.LOCKED &&
                            x.ExpireAt <= utcNow &&
                            x.Order.status == OrderStatus.PENDING &&
                            x.Order.Payment.PaymentMethod == PaymentMethod.VNPAY &&
                            x.Order.Payment.PaymentStatus == PaymentStatus.PENDING)
                .Select(x => x.OrderId)
                .Distinct()
                .ToListAsync();
        }
    }
}
