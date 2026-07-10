namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IPaymentService
    {
        Task LogWebhookAsync(string orderCode, string rawPayload, string signature, bool isVerified);
        Task<bool> MarkPaymentSuccessAsync(int orderId, string transactionNo);
        Task<bool> MarkPaymentFailedAsync(int orderId, string responseCode);
        Task<PetShop_Upgrade.DTOS.VNPay.VNPayIpnResponseDTO> ProcessVNPayIpnAsync(
            PetShop_Upgrade.DTOS.VNPay.VNPayCallbackResultDTO callback,
            string signature);
    }
}
