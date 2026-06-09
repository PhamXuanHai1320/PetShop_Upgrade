using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IColorRepository : IRepository<Color>
    {
        Task<IEnumerable<Color>> GetColorsByProductId(int productId);
        Task<IEnumerable<Color>> GetColorsByName(String ColorName);
    }
}
