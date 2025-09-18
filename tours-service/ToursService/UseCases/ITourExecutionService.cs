using FluentResults;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public interface ITourExecutionService
    {
        Result<TourExecutionDto> Create(TourExecutionDto execution);

        Result<TourExecutionDto> CompleteTourExecution(long id);

        Result<TourExecutionDto> AbandonTourExecution(long id);
        Result<TourExecutionDto> GetByTourAndTouristId(long touristId, long tourId);
        public Result<TourExecutionDto> GetActiveTourByTouristId(long touristId);

        public Result<TourExecutionDto> CompleteKeyPoint(long executionId, long keyPointId);
        public void UpdateLastActivity(long executionId);

        public ICollection<KeyPointDto> GetKeyPointsForTour(long tourId);

    }
}
