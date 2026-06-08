using MongoDB.Driver;
using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<TransactionLog> TransactionLogs
            => _database.GetCollection<TransactionLog>("TransactionLogs");
    }
}