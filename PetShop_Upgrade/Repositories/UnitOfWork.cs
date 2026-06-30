using Microsoft.EntityFrameworkCore.Storage;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;

namespace PetShop_Upgrade.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            MemberRepository = new MemberRepository(_context);
            RefreshTokenRepository = new RefreshTokenRepository(_context);
            CartRepository = new CartRepository(_context);
            ColorRepository = new ColorRepository(_context);
            CategoryRepository = new CategoryRepository(_context);
            ProductRepository = new ProductRepository(_context);
            PetVariantRepository = new PetVariantRepository(_context);
            FoodDetailRepository = new FoodDetailRepository(_context);
            ToyDetailRepository = new ToyDetailRepository(_context);
            ProductHistoryRepository = new ProductHistoryRepository(_context);
            DiscountRepository = new DiscountRepository(_context);
            CartItemRepository = new Repository<CartItem>(context);
        }

        public IMemberRepository MemberRepository { get; private set; }

        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }

        public ICartRepository CartRepository { get; private set; }
        public IColorRepository ColorRepository { get; private set; }
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public IProductHistoryRepository ProductHistoryRepository { get; private set; }

        public IDiscountRepository DiscountRepository { get; private set; }

        public IRepository<CartItem> CartItemRepository { get; private set; }

        public IPetVariantRepository PetVariantRepository { get; private set; }

        public IFoodDetailRepository FoodDetailRepository { get; private set; }

        public IToyDetailRepository ToyDetailRepository { get; private set; }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
