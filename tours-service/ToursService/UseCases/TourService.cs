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

        public Result<List<TourDto>> GetByUserId(long userId)
        {
            {
                var tours = _tourRepository.GetByAuthor(userId);

                if (tours == null || tours.Count == 0)
                {
                    return Result.Fail<List<TourDto>>("No tours found for the specified user.");
                }

                var tourDtos = tours.Select(t => new TourDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Difficulty = t.Difficulty,
                    Tags = t.Tags.Select(tag => (ToursService.Dtos.TourTags)tag).ToList(),
                    UserId = t.UserId,
                    Status = (ToursService.Dtos.TourStatus)t.Status,
                    Price = t.Price,
                    LengthInKm = t.LengthInKm,
                    //KeyPoints = t.KeyPoints.Select(kp => new KeyPointDto
                    //{
                    //    Id = kp.Id,
                    //    Name = kp.Name,
                    //    Longitude = kp.Longitude,
                    //    Latitude = kp.Latitude,
                    //    Description = kp.Description,
                    //    Image = kp.Image,
                    //    TourId = kp.TourId


                    //}).ToList()
                }).ToList();

                return Result.Ok(tourDtos);

            }
        }
    }
}
