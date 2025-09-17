using FluentResults;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public interface ITourService
    {
        Result<TourDto> Create(TourDto dto);
        Result<List<TourDto>> GetByUserId(long userId);
        public Result<List<TourDto>> GetPublished();
        Result<KeyPointDto> AddKeyPoint(long tourId, KeyPointDto keyPointDto);
        Result<List<KeyPointDto>> GetKeyPointsByTour(long tourId);
        public Result<TourForTouristDto> GetTourWithKeyPoints(long tourId);


    }
}
