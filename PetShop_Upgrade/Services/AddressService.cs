using PetShop_Upgrade.DTOS.Addresss;
using PetShop_Upgrade.Exceptions;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using System.Text.Json;

namespace PetShop_Upgrade.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAddressDataService _addressDataService;

        public AddressService(IUnitOfWork unitOfWork, IAddressDataService addressDataService)
        {
            _unitOfWork = unitOfWork;
            _addressDataService = addressDataService;
        }
        public async Task<AddressResponseDTO> CreateAddressAsync(int memberId, CreateAddressDTO createAddressDTO)
        {

            if (!_addressDataService.IsValidWardOfCity(createAddressDTO.City, createAddressDTO.Ward))
            {
                throw new BadRequestException("Địa chỉ không hợp lệ: xã/phường không thuộc tỉnh/thành phố đã chọn.");
            }

            var address = new Address
            {
                MemberId = memberId,
                City = createAddressDTO.City,
                Ward = createAddressDTO.Ward,
                AddressDetail = createAddressDTO.AddressDetail,
                PhoneNumber = createAddressDTO.PhoneNumber
            };

            await _unitOfWork.AddressRepository.Add(address);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponseDTO(address);
        }

        public async Task<AddressResponseDTO> UpdateAddressAsync(int memberId, UpdateAddressDTO updateAddressDTO)
        {
            var address = await _unitOfWork.AddressRepository.GetById(updateAddressDTO.Id);

            if (address is null)
                throw new NotFoundException("Không tìm thấy địa chỉ.");

            if (address.MemberId != memberId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa địa chỉ này.");

            if (!_addressDataService.IsValidWardOfCity(updateAddressDTO.City, updateAddressDTO.Ward))
                throw new BadRequestException("Địa chỉ không hợp lệ: xã/phường không thuộc tỉnh/thành phố đã chọn.");

            address.City = updateAddressDTO.City;
            address.Ward = updateAddressDTO.Ward;
            address.AddressDetail = updateAddressDTO.AddressDetail;
            address.PhoneNumber = updateAddressDTO.PhoneNumber;

            await _unitOfWork.AddressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponseDTO(address);
        }

        public async Task DeleteAddressAsync(int memberId, int addressId)
        {
            var address = await _unitOfWork.AddressRepository.GetById(addressId);

            if (address is null)
                throw new NotFoundException("Không tìm thấy địa chỉ.");

            if (address.MemberId != memberId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa địa chỉ này.");
            await _unitOfWork.AddressRepository.Delete(address);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<AddressResponseDTO>> GetAllAddressesByMemberIdAsync(int memberId)
        {
            var addresses = await _unitOfWork.AddressRepository.GetAllAddressesByMemberIdAsync(memberId);
            var addressResponseDTO = new List<AddressResponseDTO>();
            foreach (var address in addresses)
            {
                var addressDTO = MapToResponseDTO(address);
                addressResponseDTO.Add(addressDTO);
            }
            return addressResponseDTO;
        }
        private AddressResponseDTO MapToResponseDTO(Address address)
        {
            var city = _addressDataService.GetCityDetail(address.City);
            var ward = _addressDataService.GetWardsByCityCode(address.City)?.FirstOrDefault(w => w.Code == address.Ward);

            return new AddressResponseDTO
            {
                Id = address.Id,
                CityCode = address.City,
                CityName = city?.Name ?? string.Empty,
                WardCode = address.Ward,
                WardName = ward?.Name ?? string.Empty,
                AddressDetail = address.AddressDetail,
                PhoneNumber = address.PhoneNumber
            };
        }
    }
}
