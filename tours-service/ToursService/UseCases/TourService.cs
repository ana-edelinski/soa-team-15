using AutoMapper;
using FluentResults;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;
        private readonly IMapper _mapper;

        public TourService(ITourRepository tourRepository, IMapper mapper)
        {
            _tourRepository = tourRepository;
            _mapper = mapper;
        }

        public Result<TourDto> Create(TourDto dto)
        {
            if (dto is null)
                return Result.Fail<TourDto>("Tour DTO is null.");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Result.Fail<TourDto>("Name is required.");
            if (dto.UserId <= 0)
                return Result.Fail<TourDto>("UserId must be a positive number.");

            try
            {
                var tour = _mapper.Map<Tour>(dto);

                var created = _tourRepository.Create(tour);

                var createdDto = _mapper.Map<TourDto>(created);

                return Result.Ok(createdDto);
            }
            catch (Exception ex)
            {
                return Result.Fail<TourDto>(new Error("Failed to create tour.").CausedBy(ex));
            }
        }

    }
}
