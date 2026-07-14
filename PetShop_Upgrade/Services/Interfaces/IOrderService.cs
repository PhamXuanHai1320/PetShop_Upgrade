using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.DTOS.Order.Admin;
using PetShop_Upgrade.DTOS.Order.Client;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IOrderService
    {
        Task<CreateOrderResultDTO> CreateOrderFromCartAsync(int memberId, CreateOrderFromCartRequestDTO createOrderRequestDTO);
        Task<CreateOrderResultDTO> CreateOrderAsync(int memberId, CreateOrderItemRequestDTO createOrderRequestDTO);
        Task<OrderPreviewResponseDTO> OrderPreview(OrderPreviewRequestDTO orderPreviewRequestDTO);
        Task CancelOrderByAdminAsync(int orderId, int adminId, CancelOrderRequestDTO dto);
        Task CancelOrderByMemberAsync(int orderId, int memberId, CancelOrderRequestDTO dto);
        Task UpdateOrderStatusAsync(int orderId, int adminId, UpdateOrderStatusRequestDTO updateOrderDTO);
        Task ConfirmOrderPaymentAsync(int orderId);
        Task CancelOrderBySystemAsync(int orderId, string CancelReason);
        Task ExpirePendingVNPayOrdersAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDTO<ListOrderDTO>> GetOrderByStatus(
            OrderStatus orderStatus, int memberId,int page, int pageSize);
        Task<OrderDetailDTO> GetOrderDetail(int orderId, int memberId);
        Task<PagedResultDTO<AdminListOrderDTO>> AdminGetOrdersByFilterAsync(
            AdminOrderFilterDTO orderFilterDTO, int page, int pageSize);
        Task<AdminOrderDetailDTO> AdminGetOrderDetail(int orderId);
    }
}
