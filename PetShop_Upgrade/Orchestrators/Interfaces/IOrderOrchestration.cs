using PetShop_Upgrade.DTOS.Order;

namespace PetShop_Upgrade.Orchestrators.Interfaces
{
    public interface IOrderOrchestration
    {
        Task<CreateOrderResponseDTO> CreateOrderFromCartAsync(
            int memberId, CreateOrderFromCartRequestDTO createOrderDTO, string ipAddress);

        Task<CreateOrderResponseDTO> CreateOrderAsync(
            int memberId, CreateOrderItemRequestDTO createOrderDTO, string ipAddress);
    }
}
