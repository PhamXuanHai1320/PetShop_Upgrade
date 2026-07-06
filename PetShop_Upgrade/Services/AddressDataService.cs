using PetShop_Upgrade.DTOS.Addresss;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Services.Interfaces;
using System.Text.Json;

namespace PetShop_Upgrade.Services
{
    public class AddressDataService : IAddressDataService
    {
        private readonly List<CityDTO> _city;
        private readonly Dictionary<string, CityDTO> _cityIndex;

        public AddressDataService(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "Data", "VietNamAddress.json");
            var json = File.ReadAllText(path);

            _city = JsonSerializer.Deserialize<List<CityDTO>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CityDTO>();

            _cityIndex = _city.ToDictionary(p => p.Code, p => p);
        }
        public CityDTO? GetCityDetail(string cityCode)
        {
            return _cityIndex.TryGetValue(cityCode, out var city) ? city : null;
        }

        public IEnumerable<WardDTO>? GetWardsByCityCode(string cityCode)
        {
            return _cityIndex.TryGetValue(cityCode, out var city) ? city.Wards : null;
        }
        public IEnumerable<CitySummaryDTO> GetAllCities()
        {
            return _city.Select(p => new CitySummaryDTO
            {
                Code = p.Code,
                Name = p.Name
            });
        }
        public bool IsValidWardOfCity(string cityCode, string wardCode)
        {
            return GetWardsByCityCode(cityCode)?.Any(w => w.Code == wardCode) ?? false;
        }
    }
}
