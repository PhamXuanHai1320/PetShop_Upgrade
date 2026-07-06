using PetShop_Upgrade.DTOS.Cart;

namespace PetShop_Upgrade.DTOS.Addresss
{
    public class CityDTO
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<WardDTO> Wards { get; set; } = new();
    }
}
