using AutoMapper;
using ToursService.Domain;
using ToursService.Dtos;

namespace ToursService.Mappers
{
    public class TourReviewProfile : Profile
    {
        public TourReviewProfile()
        {
            CreateMap<TourReview, TourReviewDto>().ReverseMap();

            CreateMap<TourReviewCreateDto, TourReview>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.DateComment, opt => opt.Ignore());
        }
    }
}
