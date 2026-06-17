using Microsoft.EntityFrameworkCore.Storage;
using PetShop_Upgrade.Data;
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
        }

        public IMemberRepository MemberRepository { get; private set; }

        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }

        public ICartRepository CartRepository { get; private set; }
        public IColorRepository ColorRepository { get; private set; }
        public ICategoryRepository CategoryRepository { get; private set; }

        public IProductRepository ProductRepository { get; private set; }

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
