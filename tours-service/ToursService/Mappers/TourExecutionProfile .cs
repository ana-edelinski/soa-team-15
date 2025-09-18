using AutoMapper;
using ToursService.Domain;
using ToursService.Dtos;

namespace ToursService.Mappers
{
    public class TourExecutionProfile : Profile
    {
        public TourExecutionProfile()
        {
            // Mapiranje enum -> enum
            CreateMap<ToursService.Dtos.TourExecutionStatus, ToursService.Domain.TourExecutionStatus>()
                .ConvertUsing(src => (ToursService.Domain.TourExecutionStatus)src);

            CreateMap<ToursService.Domain.TourExecutionStatus, ToursService.Dtos.TourExecutionStatus>()
                .ConvertUsing(src => (ToursService.Dtos.TourExecutionStatus)src);

            CreateMap<CompletedKeyPoint, CompletedKeyPointDto>()
            // Ako se svojstvo zove drugačije (npr. CompletedAt), prilagodi:
            .ForMember(d => d.CompletedTime, o => o.MapFrom(s => s.CompletedTime))
            .ForMember(d => d.KeyPointId, o => o.MapFrom(s => s.KeyPointId));

            CreateMap<CompletedKeyPointDto, CompletedKeyPoint>()
                .ConstructUsing(dto => new CompletedKeyPoint(dto.KeyPointId, dto.CompletedTime));

            // DTO -> Domain
            CreateMap<TourExecutionDto, TourExecution>()
                .ConstructUsing(dto => new TourExecution(
                    dto.TourId,
                    dto.TouristId,
                    dto.LocationId,
                    dto.LastActivity,                   
                    (Domain.TourExecutionStatus)dto.Status,
                   dto.CompletedKeys != null
                    ? dto.CompletedKeys
                        .Select(k => new Domain.CompletedKeyPoint(k.KeyPointId, k.CompletedTime))
                        .ToList()
                    : new List<Domain.CompletedKeyPoint>()
           ));

            // Domain -> DTO
            CreateMap<TourExecution, TourExecutionDto>()
            .ForCtorParam("id", o => o.MapFrom(s => s.Id))
            .ForCtorParam("tourId", o => o.MapFrom(s => s.TourId))
            .ForCtorParam("touristId", o => o.MapFrom(s => s.TouristId))
            .ForCtorParam("locationId", o => o.MapFrom(s => s.LocationId))
            .ForCtorParam("lastActivity", o => o.MapFrom(s => s.LastActivity))
            .ForCtorParam("status", o => o.MapFrom(s => (ToursService.Dtos.TourExecutionStatus)s.Status))
            .ForCtorParam("completedKeys", o => o.MapFrom(s => s.CompletedKeys ?? new List<CompletedKeyPoint>()));
        }
    }
}
