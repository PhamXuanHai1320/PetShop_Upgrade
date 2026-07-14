namespace PetShop_Upgrade.DTOS.Order.Client
{
    public class UpdateShippingAddressDTO
    {
        public int OrderId { get; set; }
        public int AddressId { get; set; }
        public int MemberId { get; set; }
    }
}
