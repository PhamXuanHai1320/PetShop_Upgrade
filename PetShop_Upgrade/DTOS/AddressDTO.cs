namespace PetShop_Upgrade.DTOS
{
    public class AddressDTO
    {
        public int Id { get; set; }
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string Ward { get; set; }
        public string PhoneNumber { get; set; }
        public int MemberId { get; set; }
    }
}
