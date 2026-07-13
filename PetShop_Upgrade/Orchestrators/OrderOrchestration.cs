using PetShop_Upgrade.DTOS.Order.Client;
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
        private readonly IVNPayService _vnPayService;
        public OrderOrchestration(IOrderService orderService, IVNPayService vnPayService)
        {
            _orderService = orderService;
            _vnPayService = vnPayService;
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
                case PaymentMethod.VNPAY:
                {

                    var paymentUrl = _vnPayService.CreatePaymentUrl(
                        orderId: orderResultDTO.OrderId,
                        amount: orderResultDTO.FinalPrice,
                        orderInfo: $"Thanh toan don hang {orderResultDTO.OrderId}",
                        ipAddress: ipAddress);
                    response.PaymentUrl = paymentUrl;
                    return response;
                }
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
                case PaymentMethod.VNPAY:
                {

                    var paymentUrl = _vnPayService.CreatePaymentUrl(
                        orderId: orderResultDTO.OrderId,
                        amount: orderResultDTO.FinalPrice,
                        orderInfo: $"Thanh toan don hang {orderResultDTO.OrderId}",
                        ipAddress: ipAddress);
                    response.PaymentUrl = paymentUrl;
                    return response;
                }
                default:
                    throw new NotSupportedException($"Payment method {createOrderDTO.PaymentMethod} is not supported.");
            }
        }
    }
}
