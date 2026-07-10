using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsByIdAsync(int orderId);
        Task<Order?> GetOrderForUpdateAsync(int orderId);
    }
}
