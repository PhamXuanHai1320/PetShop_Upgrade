using AutoMapper;
using PetShop_Upgrade.DTOS;
using PetShop_Upgrade.Models;
using System.Globalization;

namespace PetShop_Upgrade.AutoMapper
{
    public class AppMapperProfile : Profile
    {
        public AppMapperProfile()
        {
            CreateMap<string, TimeOnly>().ConvertUsing(src => TimeOnly.Parse(src));
            CreateMap<string, DateOnly>().ConvertUsing(src => DateOnly.Parse(src, CultureInfo.InvariantCulture));
            CreateMap<Discount, DiscountDTO>();
            CreateMap<DiscountDTO, Discount>();
            CreateMap<CreateDiscountDTO, Discount>();
        }
    }
}
