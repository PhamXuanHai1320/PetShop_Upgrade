using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class InventoryLockRepository : Repository<InventoryLock>, Interfaces.IInventoryLockRepository
    {
        public InventoryLockRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
