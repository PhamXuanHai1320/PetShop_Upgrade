using PetShop_Upgrade.DTOS.Cart;

namespace PetShop_Upgrade.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetCartAsync(int memberId);
        Task<CartDTO> AddToCartAsync(int memberId, AddCartItemRequestDTO request);
        Task<CartDTO> UpdateCartItemAsync(int memberId, int cartItemId, UpdateCartItemRequestDTO request);
        Task<CartDTO> RemoveCartItemAsync(int memberId, int cartItemId);
        Task ClearCartAsync(int memberId);
    }
}
