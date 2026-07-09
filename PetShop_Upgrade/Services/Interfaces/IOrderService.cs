using PetShop_Upgrade.DTOS.Order;

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
    }
}
