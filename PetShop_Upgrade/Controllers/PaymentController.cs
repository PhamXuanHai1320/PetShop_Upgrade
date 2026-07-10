using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.Orchestrators;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentWebhookOrchestrator _orchestrator;
        private readonly IVNPayService _vnPayService;

        public PaymentController(IPaymentWebhookOrchestrator orchestrator, IVNPayService vnPayService)
        {
            _orchestrator = orchestrator;
            _vnPayService = vnPayService;
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VNPayIpn()
        {
            var response = await _orchestrator.HandleVNPayIpnAsync(Request.Query);
            return Ok(response);
        }

        [HttpGet("vnpay-return")]
        public IActionResult VNPayReturn()
        {
            var result = _vnPayService.ValidateCallback(Request.Query);
            return Redirect(result.IsSuccess
                ? $"https://your-frontend-domain.com/payment/success?orderId={result.OrderId}"
                : $"https://your-frontend-domain.com/payment/failed?orderId={result.OrderId}");
        }
    }
}
