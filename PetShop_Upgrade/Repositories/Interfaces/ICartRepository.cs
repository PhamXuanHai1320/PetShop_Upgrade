using PetShop_Upgrade.Models;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetCartByMemberIdAsync(int memberId);
        Task<Cart> CreateCartAsync(int memberId);
        Task<CartItem?> GetCartItemAsync(int cartId, int productColorId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
        Task AddCartItemAsync(CartItem cartItem);
        void UpdateCartItem(CartItem cartItem);
        void RemoveCartItem(CartItem cartItem);
        void RemoveCartItems(IEnumerable<CartItem> cartItems);
        Task<Dictionary<int, string>> GetColorNamesByProductColorIdsAsync(IEnumerable<int> productColorIds);
    }
}
