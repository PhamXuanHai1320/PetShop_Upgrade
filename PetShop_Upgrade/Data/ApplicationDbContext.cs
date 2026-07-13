using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetShop_Upgrade.Models;
namespace PetShop_Upgrade.Data
{
    public class ApplicationDbContext : IdentityDbContext<Member, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountCategory> DiscountCategories { get; set; }
        public DbSet<DiscountProduct> DiscountProducts { get; set; }
        public DbSet<DiscountUsage> DiscountUsages { get; set; }
        public DbSet<FoodsDetail> FoodsDetails { get; set; }
        public DbSet<InventoryLock> InventoryLocks { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<PetHealthRecord> PetHealthRecords { get; set; }
        public DbSet<PetVariant> PetVariant { get; set; }
        public DbSet<PetVaccination> PetVaccination { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductHistory> ProductHistories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ToysDetail> ToysDetails { get; set; }
        //public DbSet<PetShop_Upgrade.Models.TransactionLog> TransactionLogs { get; set; }
        public DbSet<Vaccine> Vaccine { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<PetViewingAppointment> PetViewingAppointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FoodsDetail>()
                .HasKey(fd => fd.ProductId);

            modelBuilder.Entity<ToysDetail>()
                .HasKey(fd => fd.ProductId);

            modelBuilder.Entity<PetVariant>()
                .HasKey(fd => fd.ProductId);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Addresses)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Member)
                .WithMany(m => m.RefreshTokens)
                .HasForeignKey(rt => rt.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.HashToken)
                .IsUnique();

            // Member -> Cart (1-1)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Member)
                .WithOne(m => m.Cart)
                .HasForeignKey<Cart>(c => c.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member -> Rating (1-N)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Member)
                .WithMany(m => m.Ratings)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // CART & CARTITEM
            // =====================
            // Cart -> CartItem (1-N)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem -> Product (N-1)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // ORDER
            // =====================
            // Order -> Member (N-1)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Member)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> Address (N-1)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Address)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order -> OrderDetail (1-N)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderDetail -> Product (N-1)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            // =====================
            // PetAppointment
            // =====================
            // PetAppointment -> Member (N-1)
            modelBuilder.Entity<Appointment>()
                .HasOne(o => o.Member)
                .WithMany(m => m.Appointments)
                .HasForeignKey(o => o.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
            // PetViewingAppointment -> Product (N-1)
            modelBuilder.Entity<PetViewingAppointment>()
                .HasOne(o => o.Product)
                .WithMany(m => m.PetViewingAppointments)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            // PetViewingAppointment -> Appointment (1-1)
            modelBuilder.Entity<PetViewingAppointment>()
                .HasKey(pva => pva.AppointmentId);
            modelBuilder.Entity<PetViewingAppointment>()
                .HasOne(pva => pva.Appointment)
                .WithOne()
                .HasForeignKey<PetViewingAppointment>(pva => pva.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // PAYMENT
            // =====================
            // Payment -> Order (1-1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OutboxMessage>()
                .HasIndex(x => new { x.PublishedAt, x.NextRetryAt });

            // =====================
            // PRODUCT
            // =====================
            // Product -> Category (N-1)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> ProductImage (1-N)
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> ProductHistory (1-N)
            modelBuilder.Entity<ProductHistory>()
                .HasOne(ph => ph.Product)
                .WithMany(p => p.ProductHistories)
                .HasForeignKey(ph => ph.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductHistory>()
                .HasOne(ph => ph.Member)
                .WithMany(m => m.ProductHistories)
                .HasForeignKey(ph => ph.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> ProductColor (1-N) — đã có, giữ lại
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductColors)
                .WithOne(pc => pc.Product)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductColor -> Color (N-1)
            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Color)
                .WithMany(c => c.ProductColors)
                .HasForeignKey(pc => pc.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> Rating (1-N)
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // PRODUCT DETAIL (TPT / optional sub-tables)
            // =====================
            // ToysDetail -> Product (1-1)
            modelBuilder.Entity<ToysDetail>()
                .HasOne(td => td.Product)
                .WithOne(p => p.ToysDetails)
                .HasForeignKey<ToysDetail>(td => td.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // FoodsDetail -> Product (1-1)
            modelBuilder.Entity<FoodsDetail>()
                .HasOne(fd => fd.Product)
                .WithOne(p => p.FoodsDetails)
                .HasForeignKey<FoodsDetail>(fd => fd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // PetVariant -> Product (N-1)
            modelBuilder.Entity<PetVariant>()
                .HasOne(pv => pv.Product)
                .WithOne(p => p.PetVariant)
                .HasForeignKey<PetVariant>(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // PetHealthRecord -> Product (N-1)
            modelBuilder.Entity<PetHealthRecord>()
                .HasOne(phr => phr.PetVariant)
                .WithMany(p => p.PetHealthRecords)
                .HasForeignKey(phr => phr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // PET VACCINATION & VACCINE
            // =====================
            // PetVaccination -> Vaccine (N-1)
            modelBuilder.Entity<PetVaccination>()
                .HasOne(pv => pv.Vaccine)
                .WithMany(v => v.PetVaccinations)
                .HasForeignKey(pv => pv.VaccineId)
                .OnDelete(DeleteBehavior.Restrict);

            // PetVaccination -> Product (N-1) — con thú cưng
            modelBuilder.Entity<PetVaccination>()
                .HasOne(pv => pv.PetVariant)
                .WithMany(p => p.PetVaccinations)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================
            // DISCOUNT
            // =====================
            modelBuilder.Entity<Discount>()
                .HasIndex(d => d.Code)
                .IsUnique()
                .HasFilter("[Code] IS NOT NULL");
            // DiscountProduct (junction: Discount <-> Product)
            modelBuilder.Entity<DiscountProduct>()
                .HasOne(dp => dp.Discount)
                .WithMany(d => d.DiscountProducts)
                .HasForeignKey(dp => dp.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiscountProduct>()
                .HasOne(dp => dp.Product)
                .WithMany(p => p.DiscountProducts)
                .HasForeignKey(dp => dp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // DiscountCategory (junction: Discount <-> Category)
            modelBuilder.Entity<DiscountCategory>()
                .HasOne(dc => dc.Discount)
                .WithMany(d => d.DiscountCategories)
                .HasForeignKey(dc => dc.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiscountCategory>()
                .HasOne(dc => dc.Category)
                .WithMany(c => c.DiscountCategories)
                .HasForeignKey(dc => dc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // DiscountUsage -> Discount (N-1)
            modelBuilder.Entity<DiscountUsage>()
                .HasOne(du => du.Discount)
                .WithMany(d => d.DiscountUsages)
                .HasForeignKey(du => du.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            // DiscountUsage -> Member (N-1)
            modelBuilder.Entity<DiscountUsage>()
                .HasOne(du => du.Member)
                .WithMany(m => m.DiscountUsages)
                .HasForeignKey(du => du.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // DiscountUsage -> Order (N-1)
            modelBuilder.Entity<DiscountUsage>()
                .HasOne(du => du.Order)
                .WithMany(o => o.DiscountUsages)
                .HasForeignKey(du => du.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Discount -> Member (N-1) — người tạo mã giảm giá
            modelBuilder.Entity<Discount>()
                .HasOne(d => d.Member)
                .WithMany(m => m.Discounts)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================
            // INVENTORY LOCK
            // =====================
            // InventoryLock -> ProductColor (N-1)
            modelBuilder.Entity<InventoryLock>()
                .HasOne(il => il.ProductColor)
                .WithMany(pc => pc.InventoryLocks)
                .HasForeignKey(il => il.ProductColorId)
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryLock -> Order (N-1)
            modelBuilder.Entity<InventoryLock>()
                .HasOne(il => il.Order)
                .WithMany(o => o.InventoryLocks)
                .HasForeignKey(il => il.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "Employee", NormalizedName = "EMPLOYEE" },
                new IdentityRole<int> { Id = 3, Name = "Customer", NormalizedName = "CUSTOMER" }
            );
        }
    }
}
