using PetShop_Upgrade.DTOS;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface IColorService
    {
        Task<IEnumerable<ColorDTO>> GetAllColorsAsync();
        Task<ColorDTO> GetColorByIdAsync(int colorId); 
        Task<IEnumerable<ProductColorRequestDTO>> GetColorsByProductIdAsync(int productId);
        Task<IEnumerable<ColorDTO>> GetColorsByName(string ColorName);
        Task<ColorDTO> CreateColorAsync(ColorDTO colorDTO);
        Task<ColorDTO> UpdateColorAsync(int id, ColorDTO colorDTO);
        Task DeleteColorAsync(int id);
    }
}
