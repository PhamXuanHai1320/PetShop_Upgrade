using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Cart?> GetCartByMemberIdAsync(int memberId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(c => c.CartItems)
                .Where(c => c.MemberId == memberId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        public async Task<Cart> CreateCartAsync(int memberId)
        {
            var cart = new Cart
            {
                MemberId = memberId,
                TotalPrice = 0,
                CreatedAt = DateTime.Now,
                CartItems = []
            };

            await _context.Carts.AddAsync(cart);
            return cart;
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productColorId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductColorId == productColorId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        public void UpdateCartItem(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
        }

        public void RemoveCartItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }

        public void RemoveCartItems(IEnumerable<CartItem> cartItems)
        {
            _context.CartItems.RemoveRange(cartItems);
        }
        public async Task<Dictionary<int, string>> GetColorNamesByProductColorIdsAsync(IEnumerable<int> productColorIds)
        {
            var ids = productColorIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, string>();

            return await _context.ProductColors
                .Where(pc => ids.Contains(pc.Id))
                .Select(pc => new { pc.Id, ColorName = pc.Color.ColorName })
                .ToDictionaryAsync(x => x.Id, x => x.ColorName);
        }
    }
}
