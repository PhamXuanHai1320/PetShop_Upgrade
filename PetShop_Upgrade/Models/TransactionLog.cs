using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetShop_Upgrade.Models
{
    public class TransactionLog
    {
        public TransactionLog() { }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TransactionId { get; set; } = null!;
        public string API { get; set; }
        public string Status { get; set; }
        public string Data { get; set; } = string.Empty;  
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
