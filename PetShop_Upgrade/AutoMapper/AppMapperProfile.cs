using AutoMapper;
using PetShop_Upgrade.DTOS.Cart;
using PetShop_Upgrade.DTOS.Colors;
using PetShop_Upgrade.DTOS.Discounts;
using PetShop_Upgrade.DTOS.Products.Admin;
using PetShop_Upgrade.DTOS.Products.Client;
using PetShop_Upgrade.Models;
using System.Globalization;
using static PetShop_Upgrade.Models.Enum;

namespace PetShop_Upgrade.AutoMapper
{
    public class AppMapperProfile : Profile
    {
        private readonly string _baseUrl;
        private readonly string _bucketName;

        public AppMapperProfile(IConfiguration config)
        {
            _baseUrl = config["MinIO:BaseUrl"];
            _bucketName = config["MinIO:BucketName"];
            // --- Các cấu hình Convert cơ bản ---
            CreateMap<string, TimeOnly>().ConvertUsing(src => TimeOnly.Parse(src));
            CreateMap<string, DateOnly>().ConvertUsing(src => DateOnly.Parse(src, CultureInfo.InvariantCulture));
            // --- Cấu hình cho Module Discount ---
            CreateMap<Discount, DiscountDTO>();
            CreateMap<DiscountDTO, Discount>();
            CreateMap<CreateDiscountDTO, Discount>();
            CreateMap<Discount, DiscountItemsDTO>();
            // --- Cấu hình cho Module Product ---
            CreateMap<Product, ProductResponseRequestDTO>();
            CreateMap<Product, AdminProductItemDTO>()
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src =>
                        src.ProductImages.FirstOrDefault(img => img.IsMain == IsMain.MAIN) != null
                            ? $"{_baseUrl}/{_bucketName}/{src.ProductImages.First(img => img.IsMain == IsMain.MAIN).ImageUrl}"
                            : null))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Product, ProductDetailDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src =>
                        src.Ratings.Any() ? src.Ratings.Average(r => r.Ratting) : 0))
                .ForMember(dest => dest.RatingCount,
                    opt => opt.MapFrom(src => src.Ratings.Count));

            CreateMap<Product, ProductItemsDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src =>
                        src.Ratings.Any() ? src.Ratings.Average(r => r.Ratting) : 0))
                .ForMember(dest => dest.RatingCount,
                    opt => opt.MapFrom(src => src.Ratings.Count))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src =>
                        src.ProductImages.FirstOrDefault(img => img.IsMain == IsMain.MAIN) != null
                            ? $"{_baseUrl}/{_bucketName}/{src.ProductImages.First(img => img.IsMain == IsMain.MAIN).ImageUrl}"
                            : null));
            // --- Cấu hình cho Sub-Object (Image, Color) ---
            CreateMap<ProductImage, ProductImageRequestDTO>()
                .ForMember(dest => dest.ImageUrl,
                opt => opt.MapFrom(src =>
                    $"{_baseUrl}/{_bucketName}/{src.ImageUrl}"));
            CreateMap<ProductImage, ProductImagesResponseDTO>()
                .ForMember(dest => dest.ImageUrl,
                opt => opt.MapFrom(src =>
                    $"{_baseUrl}/{_bucketName}/{src.ImageUrl}"));
            CreateMap<ProductColor, ProductColorRequestDTO>()
                .ForMember(dest => dest.ColorName,
                opt => opt.MapFrom(src => src.Color.ColorName));
            CreateMap<ProductColor, ProductColorResponseDTO>()
                .ForMember(dest => dest.ColorName,
                opt => opt.MapFrom(src => src.Color.ColorName));
            // --- Cấu hình cho Module Cart ---
            CreateMap<Cart, CartDTO>();
            CreateMap<CartItem, CartItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductImageUrl,
                    opt => opt.MapFrom(src =>
                        src.Product.ProductImages.FirstOrDefault(img => img.IsMain == IsMain.MAIN) != null
                            ? $"{_baseUrl}/{_bucketName}/{src.Product.ProductImages.First(img => img.IsMain == IsMain.MAIN).ImageUrl}"
                            : null));
        }
    }
}
