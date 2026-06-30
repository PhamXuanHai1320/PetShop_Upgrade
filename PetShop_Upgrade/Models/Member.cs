using Microsoft.AspNetCore.Identity;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.Models
{
    public class Member : IdentityUser<int>
    {
        public Member() { }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreateAt { get; set; }
        public ICollection<Address> Addresses { get; set; } = [];
        public Cart Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = [];
        public ICollection<Rating> Ratings { get; set; } = [];
        public ICollection<ProductHistory> ProductHistories { get; set; } = [];
        public ICollection<DiscountUsage> DiscountUsages { get; set; } = [];
        public ICollection<Discount> Discounts { get; set; } = [];
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
