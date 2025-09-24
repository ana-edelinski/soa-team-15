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

        Result Publish(long tourId, long authorId);
        Result Archive(long tourId, long authorId);
        Result Reactivate(long tourId, long authorId);
        Result<TourDto> GetById(long tourId);

        Result<double> UpdateTourKM(long tourId, List<KeyPointDto> keyPoints);
        Result<List<TourDto>> GetAll();
        Result<List<TourDto>> GetAllIncludingUnpublished();



        Result<List<TourPublicDto>> GetPublishedPublic();
        Result<TourPublicDto> GetPublicTour(long tourId);

    }
}
