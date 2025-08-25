using AutoMapper;
using ToursService.Domain;
using ToursService.Dtos;

namespace ToursService.Mappers
{
    public class TourProfile : Profile
    {
        public TourProfile()
        {
            CreateMap<ToursService.Dtos.TourTags, ToursService.Domain.TourTags>().ReverseMap();
            CreateMap<ToursService.Domain.TourStatus, ToursService.Dtos.TourStatus>().ReverseMap();

            CreateMap<TourDto, Tour>()
                .ForCtorParam("name", opt => opt.MapFrom(src => src.Name))
                .ForCtorParam("description", opt => opt.MapFrom(src => src.Description))
                .ForCtorParam("difficulty", opt => opt.MapFrom(src => src.Difficulty))
                .ForCtorParam("price", opt => opt.MapFrom(_ => 0d)) // početna cena = 0
                .ForCtorParam("tags", opt => opt.MapFrom(src => src.Tags ?? new List<ToursService.Dtos.TourTags>()))
                .ForCtorParam("userId", opt => opt.MapFrom(src => src.UserId));

            CreateMap<Tour, TourDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.Difficulty, o => o.MapFrom(s => s.Difficulty))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))   // 0
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Tags, o => o.MapFrom(s => s.Tags))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status))
                .ForMember(d => d.LengthInKm, o => o.MapFrom(s => s.LengthInKm))
                .ForMember(d => d.PublishedTime, o => o.MapFrom(s => s.PublishedTime))
                .ForMember(d => d.ArchiveTime, o => o.MapFrom(s => s.ArchiveTime));
        }
    }
}
