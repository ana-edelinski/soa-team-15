using FluentResults;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public interface ITourService
    {
        Result<TourDto> Create(TourDto dto);
        Result<List<TourDto>> GetByUserId(long userId);
        Result<KeyPointDto> AddKeyPoint(long tourId, KeyPointDto keyPointDto);
        Result<List<KeyPointDto>> GetKeyPointsByTour(long tourId);


    }
}
