using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetShop_Upgrade.Data;
using PetShop_Upgrade.Models;
using PetShop_Upgrade.Repositories.Interfaces;
using PetShop_Upgrade.Repositories;
using PetShop_Upgrade.Services.Interfaces;
using PetShop_Upgrade.Services;
using PetShop_Upgrade.Utils;
using System.Text;
using PetShop_Upgrade.Utils.Interfaces;
using PetShop_Upgrade.Middlewares;
using PetShop_Upgrade.AutoMapper;
using Minio;
using Minio.DataModel.Args;
using PetShop_Upgrade.Orchestrators.Interfaces;
using PetShop_Upgrade.Orchestrators;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile(new AppMapperProfile(builder.Configuration));
});

// Add services to the container.
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ITransactionLogService, TransactionLogService>();
builder.Services.AddScoped<IColorService, ColorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFoodsDetailService, FoodsDetailService>();
builder.Services.AddScoped<IToysDetailService, ToysDetailService>();
builder.Services.AddScoped<IPetVariantService, PetVariantService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAddressDataService, AddressDataService>();
builder.Services.AddScoped<IMinioService, MinioService>();

// Add Repository to the container.
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ITransactionLogRepository, TransactionLogRepository>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddScoped<IFoodDetailRepository, FoodDetailRepository>();
builder.Services.AddScoped<IToyDetailRepository, ToyDetailRepository>();
builder.Services.AddScoped<IPetVariantRepository, PetVariantRepository>();
builder.Services.AddScoped<IProductHistoryRepository, ProductHistoryRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductColorRepository, ProductColorRepository>();
builder.Services.AddScoped<IInventoryLockRepository, InventoryLockRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ITokenHelper, TokenHelper>();
builder.Services.AddScoped<IProductOrchestrator, ProductOrchestrator>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Member, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;

    options.Lockout.AllowedForNewUsers = true; // để Admin khóa được User
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Khóa 15p nếu nhập sai
    //options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Quan trọng để check token hết hạn
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Cấu hình MinIO
var minioConfig = builder.Configuration.GetSection("MinIO");
var endpoint = minioConfig["Endpoint"];
var accessKey = minioConfig["AccessKey"];
var secretKey = minioConfig["SecretKey"];

builder.Services.AddSingleton<IMinioClient>(sp =>
{
    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .Build();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Gọi hàm tạo Admin (Role đã được EF Core tự tạo qua Migration)
        await DbSeeder.SeedAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo tài khoản Admin mặc định.");
    }
}
// Cấu hình chính sách truy cập cho bucket MinIO
using (var scope = app.Services.CreateScope())
{
    var minioClient = scope.ServiceProvider.GetRequiredService<IMinioClient>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var bucketName = config["MinIO:BucketName"];

    var policy = $@"{{
        ""Version"": ""2012-10-17"",
        ""Statement"": [{{
            ""Effect"": ""Allow"",
            ""Principal"": {{""AWS"": [""*""]}},
            ""Action"": [""s3:GetObject""],
            ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
        }}]
    }}";

    await minioClient.SetPolicyAsync(
        new SetPolicyArgs()
            .WithBucket(bucketName)
            .WithPolicy(policy));
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TransactionLoggingMiddleware>();

app.MapControllers();

app.Run();
