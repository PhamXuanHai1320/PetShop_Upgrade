using PetShop_Upgrade.DTOS.VNPay;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string ipAddress);
        VNPayCallbackResultDTO ValidateCallback(IQueryCollection query);
    }
}
