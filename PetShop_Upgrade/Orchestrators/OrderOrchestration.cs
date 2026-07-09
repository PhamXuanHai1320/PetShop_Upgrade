using PetShop_Upgrade.DTOS.Order;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services;
using PetShop_Upgrade.Services.Interfaces;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Orchestrators
{
    public class OrderOrchestration : IOrderOrchestration
    {
        private readonly IOrderService _orderService;
        public OrderOrchestration(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public async Task<CreateOrderResponseDTO> CreateOrderFromCartAsync(int memberId, CreateOrderFromCartRequestDTO createOrderDTO, string ipAddress)
        {
            var orderResultDTO = await _orderService.CreateOrderFromCartAsync(memberId, createOrderDTO);
            var response = new CreateOrderResponseDTO
            {
                OrderId = orderResultDTO.OrderId,
            };
            switch(createOrderDTO.PaymentMethod)
            {
                case PaymentMethod.CASH:
                    return response;
                default:
                    throw new NotSupportedException($"Payment method {createOrderDTO.PaymentMethod} is not supported.");
            }
        }

        public async Task<CreateOrderResponseDTO> CreateOrderAsync(int memberId, CreateOrderItemRequestDTO createOrderDTO, string ipAddress)
        {
            var orderResultDTO = await _orderService.CreateOrderAsync(memberId, createOrderDTO);
            var response = new CreateOrderResponseDTO
            {
                OrderId = orderResultDTO.OrderId
            };
            switch (createOrderDTO.PaymentMethod)
            {
                case PaymentMethod.CASH:
                    return response;
                default:
                    throw new NotSupportedException($"Payment method {createOrderDTO.PaymentMethod} is not supported.");
            }
        }
    }
}
