using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Order.Admin;
using PetShop_Upgrade.Models;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsByIdAsync(int orderId);
        Task<Order?> GetOrderForUpdateAsync(int orderId);
        Task<PagedData<Order>> AdminGetOrdersByFilterAsync(AdminOrderFilterDTO orderFilterDTO, int page, int pageSize);
        Task<PagedData<Order>> GetOrdersByOrderStatusAsync(OrderStatus status, int memberId, int page, int pageSize);
        Task<Order?> AdminGetOrderDetail(int orderId);
        Task<Order?> GetOrderDetail(int orderId, int memberId);
    }
}
