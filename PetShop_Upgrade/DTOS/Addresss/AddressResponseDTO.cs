namespace PetShop_Upgrade.DTOS.Addresss
{
    public class AddressResponseDTO
    {
        public int Id { get; set; }
        public string CityCode { get; set; } = default!;
        public string CityName { get; set; } = default!;
        public string WardCode { get; set; } = default!;
        public string WardName { get; set; } = default!;
        public string AddressDetail { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }
}
