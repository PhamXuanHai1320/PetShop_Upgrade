using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface ITransactionLogService
    {
        Task LogAsync(TransactionLog log);
    }
}
