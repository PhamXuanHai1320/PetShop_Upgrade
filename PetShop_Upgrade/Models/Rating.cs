using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Rating
    {
        public Rating() { }
        public int Id { get; set; }
        public string Content { get; set; }
        public int Ratting { get; set; }
        public IsActive IsActive { get; set; } = IsActive.ACTIVE; // 0: InActive; 1: Active
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
