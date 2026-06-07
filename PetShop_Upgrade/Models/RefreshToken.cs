  namespace PetShop_Upgrade.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string HashToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;
    }
}
