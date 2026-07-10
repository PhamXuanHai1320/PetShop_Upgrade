using PetShop_Upgrade.DTOS.VNPay;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Orchestrators
{
    public class PaymentWebhookOrchestrator : IPaymentWebhookOrchestrator
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IVNPayService _vnPayService;
        private readonly ILogger<PaymentWebhookOrchestrator> _logger;

        public PaymentWebhookOrchestrator(
            IPaymentService paymentService,
            IOrderService orderService,
            IVNPayService vnPayService,
            ILogger<PaymentWebhookOrchestrator> logger)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _vnPayService = vnPayService;
            _logger = logger;
        }

        public async Task<VNPayIpnResponseDTO> HandleVNPayIpnAsync(IQueryCollection query)
        {
            var result = _vnPayService.ValidateCallback(query);

            return await _paymentService.ProcessVNPayIpnAsync(
                result, query["vnp_SecureHash"].ToString());
        }
    }
}
