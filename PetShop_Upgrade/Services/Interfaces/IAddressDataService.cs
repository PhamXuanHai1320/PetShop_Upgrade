using PetShop_Upgrade.DTOS.Addresss;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IAddressDataService
    {
        IEnumerable<CitySummaryDTO> GetAllCities();
        IEnumerable<WardDTO>? GetWardsByCityCode(string cityCode);
        CityDTO? GetCityDetail(string cityCode);
        bool IsValidWardOfCity(string cityCode, string wardCode);
    }
}
