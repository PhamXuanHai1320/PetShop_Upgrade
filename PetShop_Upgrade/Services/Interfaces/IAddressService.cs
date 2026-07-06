using PetShop_Upgrade.DTOS.Addresss;
using PetShop_Upgrade.DTOS.Cart;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IAddressService
    {
        Task<AddressResponseDTO> CreateAddressAsync(int memberId, CreateAddressDTO createAddressDTO);
        Task<AddressResponseDTO> UpdateAddressAsync(int memberId, UpdateAddressDTO updateAddressDTO);
        Task DeleteAddressAsync(int memberId, int addressId);
        Task<IEnumerable<AddressResponseDTO>> GetAllAddressesByMemberIdAsync(int memberId);
    }
}
