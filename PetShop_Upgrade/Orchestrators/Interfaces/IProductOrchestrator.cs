using PetShop_Upgrade.DTOS;

namespace PetShop_Upgrade.Orchestrators.Interfaces
{
    public interface IProductOrchestrator
    {
        Task CreatePetAsync(CreatePetDTO createPetDTO);
        Task CreateToyAsync(CreateToyDTO createToyDTO);
        Task CreateFoodAsync(CreateFoodDTO createFoodDTO);
        Task UpdatePetAsync(UpdatePetDTO updatePetDTO, int productId);
        Task UpdateToyAsync(UpdateToyDTO updateToyDTO, int productId);
        Task UpdateFoodAsync(UpdateFoodDTO updateFoodDTO, int productId);
    }
}
