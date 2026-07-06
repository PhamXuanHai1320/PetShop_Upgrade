using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.DTOS.Addresss;
using PetShop_Upgrade.Services.Interfaces;
using System.Threading.Tasks;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private int memberId => int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAddresses()
        {
            var addresses = await _addressService.GetAllAddressesByMemberIdAsync(memberId);
            return Ok(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDTO createAddressDTO)
        {
            var result = await _addressService.CreateAddressAsync(memberId, createAddressDTO);
            return CreatedAtAction(nameof(GetAllAddresses), new { id = result.Id }, result);
        }

        [HttpPut("{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateAddressDTO updateAddressDTO)
        {
            updateAddressDTO.Id = addressId;
            var result = await _addressService.UpdateAddressAsync(memberId, updateAddressDTO);
            return Ok(result);
        }

        [HttpDelete("{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            await _addressService.DeleteAddressAsync(memberId, addressId);
            return NoContent();
        }

    }
}
