using MongoDB.Driver;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class TransactionLogRepository : ITransactionLogRepository
    {
        private readonly MongoDbContext _mongo;

        public TransactionLogRepository(MongoDbContext mongo)
        {
            _mongo = mongo;
        }

        public async Task AddTransactionLogAsync(TransactionLog log)
        {
            await _mongo.TransactionLogs.InsertOneAsync(log);
        }
    }
}
