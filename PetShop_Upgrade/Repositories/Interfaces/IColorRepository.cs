using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IColorRepository : IRepository<Color>
    {
        Task<IEnumerable<Color>> GetColorsByProductIdAsync(int productId);
        Task<IEnumerable<Color>> GetColorsByNameAsync(string ColorName);
        Task<IEnumerable<Color>> GetAllColorsAsync();
        Task<Color> GetColorByIdAsync(int colorId);
        Task<bool> HasProductColorByColorIdAsync(int colorId);
    }
}
