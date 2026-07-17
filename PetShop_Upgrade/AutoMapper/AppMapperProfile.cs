using AutoMapper;
using PetShop_Upgrade.DTOS.Appointments;
using PetShop_Upgrade.DTOS.Appointments.Admin;
using PetShop_Upgrade.DTOS.Appointments.Client;
using PetShop_Upgrade.DTOS.Cart;
using PetShop_Upgrade.DTOS.Colors;
using PetShop_Upgrade.DTOS.Discounts;
using PetShop_Upgrade.DTOS.Order.Admin;
using PetShop_Upgrade.DTOS.Order.Client;
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
            // --- Cấu hình cho Module Order ---
            CreateMap<Order, OrderDetailDTO>()
                .ForMember(dest => dest.OrderCode,
                otp => otp.MapFrom(src => $"PS{src.Id:D6}"));
            CreateMap<OrderDetail, OrderItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductImageUrl,
                    opt => opt.MapFrom(src =>
                        src.Product.ProductImages
                            .Where(img => img.IsMain == IsMain.MAIN)
                            .Select(img => $"{_baseUrl}/{_bucketName}/{img.ImageUrl}")
                            .FirstOrDefault()))
                .ForMember(dest => dest.ProductColorName,
                    opt => opt.MapFrom(src =>
                        src.Product.ProductColors
                            .Where(pc => pc.Id == src.ProductColorId)
                            .Select(pc => pc.Color.ColorName)
                            .FirstOrDefault()));
            CreateMap<Order, AdminOrderDetailDTO>()
                .ForMember(dest => dest.OrderCode,
                    otp => otp.MapFrom(src => $"PS{src.Id:D6}"))
                .ForMember(dest => dest.MemberOrderName,
                    opt => opt.MapFrom(src => $"{src.Member.FirstName} {src.Member.LastName}"))
                .ForMember(dest => dest.MemberOrderEmail,
                    opt => opt.MapFrom(src => src.Member.Email))
                .ForMember(dest => dest.MemberOrderPhoneNumber,
                    opt => opt.MapFrom(src => src.Member.PhoneNumber))
                .ForMember(dest => dest.PaymentMethod,
                    opt => opt.MapFrom(src => src.Payment.PaymentMethod))
                .ForMember(dest => dest.PaymentStatus,
                    opt => opt.MapFrom(src => src.Payment.PaymentStatus));
            // --- Cấu hình cho Module Appointment ---
            CreateMap<Appointment, AdminAppointmentDetailDTO>()
                .ForMember(dest => dest.MemberName,
                    opt => opt.MapFrom(src => $"{src.Member.FirstName} {src.Member.LastName}".Trim()))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Member.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.Member.PhoneNumber))
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.ProductId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.Product.ProductName))
                .ForMember(dest => dest.ProductColorName,
                    opt => opt.MapFrom(src =>
                        src.PetViewingAppointment.Product.ProductColors
                            .Where(pc => pc.Id == src.PetViewingAppointment.ProductColorId)
                            .Select(pc => pc.Color.ColorName)
                            .FirstOrDefault()));
            CreateMap<Appointment, AppointmentDetailDTO>()
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.ProductId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.Product.ProductName))
                .ForMember(dest => dest.ProductColorName,
                    opt => opt.MapFrom(src =>
                        src.PetViewingAppointment.Product.ProductColors
                            .Where(pc => pc.Id == src.PetViewingAppointment.ProductColorId)
                            .Select(pc => pc.Color.ColorName)
                            .FirstOrDefault()));
            CreateMap<Appointment, AdminAppointmentItemDTO>()
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.ProductId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.Product.ProductName))
                .ForMember(dest => dest.MemberName,
                    opt => opt.MapFrom(src => $"{src.Member.FirstName} {src.Member.LastName}".Trim()));
            CreateMap<Appointment, AppointmentItemDTO>()
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.ProductId))
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.PetViewingAppointment.Product.ProductName));
        }
    }
}
