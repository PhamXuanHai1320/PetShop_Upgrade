namespace PetShop_Upgrade.Models
{
    public class Address
    {
        public Address() { }
        public int Id { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string Ward { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public ICollection<Order> Orders { get; set; } = [];
    }
}
