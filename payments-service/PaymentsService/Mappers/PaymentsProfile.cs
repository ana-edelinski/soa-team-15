using AutoMapper;
using PaymentsService.Domain;
using PaymentsService.Dtos;

namespace PaymentsService.Mappers
{
    public class PaymentsProfile : Profile
    {
        public PaymentsProfile()
        {
            // --------------------
            // OrderItem
            // --------------------
            CreateMap<OrderItemDto, OrderItem>()
                .ForCtorParam("tourName", opt => opt.MapFrom(src => src.TourName))
                .ForCtorParam("price", opt => opt.MapFrom(src => src.Price))
                .ForCtorParam("tourId", opt => opt.MapFrom(src => src.TourId))
                .ForCtorParam("cartId", opt => opt.MapFrom(src => src.CartId));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.TourName, o => o.MapFrom(s => s.TourName))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.TourId, o => o.MapFrom(s => s.TourId))
                .ForMember(d => d.CartId, o => o.MapFrom(s => s.CartId));

            // --------------------
            // ShoppingCart
            // --------------------
            CreateMap<ShoppingCartDto, ShoppingCart>()
                .ForCtorParam("userId", opt => opt.MapFrom(src => src.UserId));

            CreateMap<ShoppingCart, ShoppingCartDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
                .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.Items.Sum(i => i.Price)));
        }
    }
}