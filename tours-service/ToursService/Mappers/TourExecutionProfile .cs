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

            // DTO -> Domain
            CreateMap<TourExecutionDto, TourExecution>()
                .ConstructUsing(dto => new TourExecution(
                    dto.TourId,
                    dto.TouristId,
                    dto.LocationId,
                    dto.LastActivity,
                    (Domain.TourExecutionStatus)dto.Status));

            // Domain -> DTO
            CreateMap<TourExecution, TourExecutionDto>();
        }
    }
}
