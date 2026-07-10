namespace PetShop_Upgrade.Orchestrators.Interfaces
{
    public interface IPaymentWebhookOrchestrator
    {
        Task<PetShop_Upgrade.DTOS.VNPay.VNPayIpnResponseDTO> HandleVNPayIpnAsync(IQueryCollection query);
    }
}
