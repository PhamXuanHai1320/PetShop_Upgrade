using Microsoft.EntityFrameworkCore.Storage;
using PetShop_Upgrade.Models;
using System.Linq.Expressions;

namespace PetShop_Upgrade.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        IMemberRepository MemberRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        ICartRepository CartRepository { get; }
        IColorRepository ColorRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IPetVariantRepository PetVariantRepository { get; }
        IFoodDetailRepository FoodDetailRepository { get; }
        IToyDetailRepository ToyDetailRepository { get; }
        IProductRepository ProductRepository { get; }
        IProductHistoryRepository ProductHistoryRepository { get; }
        IDiscountRepository DiscountRepository { get; }
        IRepository<CartItem> CartItemRepository { get; }
        IProductColorRepository ProductColorRepository { get; }
        IInventoryLockRepository InventoryLockRepository { get; }
        IRepository<OrderDetail> OrderDetailRepository { get; }
        IOrderRepository OrderRepository { get; }
        IAddressRepository AddressRepository { get; }

    }
}
