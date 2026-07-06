using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShop_Upgrade.Services.Interfaces;

namespace PetShop_Upgrade.Controllers
{
    [ApiController]
    [Route("api/city")]
    public class AddressDataController : ControllerBase
    {
        private readonly IAddressDataService _addressDataService;

        public AddressDataController(IAddressDataService addressDataService)
        {
            _addressDataService = addressDataService;
        }
        [HttpGet("{cityCode}/wards")]
        [ResponseCache(Duration = 86400)]
        public IActionResult GetWardsByCityCode(string cityCode)
        {
            var wards = _addressDataService.GetWardsByCityCode(cityCode);

            if (wards is null)
                return NotFound(new { message = $"Không tìm thấy tỉnh/thành phố với mã '{cityCode}'." });

            return Ok(wards);
        }
        [HttpGet()]
        [ResponseCache(Duration = 86400)]
        public IActionResult GetAllCities()
        {
            var provinces = _addressDataService.GetAllCities();
            return Ok(provinces);
        }
        [HttpGet("{cityCode}")]
        [ResponseCache(Duration = 86400)]
        public IActionResult GetCityDetailByCode(string cityCode)
        {
            var city = _addressDataService.GetCityDetail(cityCode);
            if (city is null)
                return NotFound(new { message = $"Không tìm thấy tỉnh/thành phố với mã '{cityCode}'." });
            return Ok(city);
        }
    }
}
