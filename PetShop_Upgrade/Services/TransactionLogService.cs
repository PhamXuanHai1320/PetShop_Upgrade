using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Services
{
    public class TransactionLogService : ITransactionLogService
    {
        private readonly ITransactionLogRepository _repo;

        public TransactionLogService(ITransactionLogRepository repo)
        {
            _repo = repo;
        }

        public async Task LogAsync(TransactionLog log)
        {
            await _repo.AddTransactionLogAsync(log);
        }
    }
}
