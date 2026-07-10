using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IInventoryLockRepository : IRepository<InventoryLock>
    {
        Task<List<int>> GetExpiredPendingOrderIdsAsync(DateTime utcNow);
    }
}
