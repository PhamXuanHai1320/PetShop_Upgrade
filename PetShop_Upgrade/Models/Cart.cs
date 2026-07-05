namespace PetShop_Upgrade.Models
{
    public class Cart
    {
        public Cart() { }
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}
