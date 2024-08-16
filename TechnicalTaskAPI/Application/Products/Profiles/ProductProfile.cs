using AutoMapper;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.Application.Products.Models;

namespace TechnicalTaskAPI.Application.Products.Profiles
{
    public class ProductProfile : Profile
    {
        public static decimal VatRate { get; set; }

        public ProductProfile()
        {
            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.PriceWithVAT,
                           opt => opt.MapFrom(src => CalculatePriceWithVAT(src.PricePerUnit, src.Quantity)));
        }

        private decimal CalculatePriceWithVAT(decimal pricePerUnit, int quantity)
        {
            return (quantity * pricePerUnit) * (1 + VatRate);
        }
    }
}
