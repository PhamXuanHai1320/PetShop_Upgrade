using System.ComponentModel.DataAnnotations;

namespace PetShop_Upgrade.DTOS.Addresss
{
    public class UpdateAddressDTO
    {
        public int Id { get; set; }
        public string AddressDetail { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Ward { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public int MemberId { get; set; }
    }
}
