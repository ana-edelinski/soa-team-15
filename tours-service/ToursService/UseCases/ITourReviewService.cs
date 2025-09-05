using FluentResults;
using ToursService.Dtos;

namespace ToursService.UseCases
{
    public interface ITourReviewService
    {
        Result<TourReviewDto> Create(TourReviewCreateDto dto, long? requesterIdFromClaims = null);
        Result<List<TourReviewDto>> GetByTour(long tourId);
        Result<List<TourReviewDto>> GetByTourist(long touristId);
        Result<PagedResult<TourReviewDto>> GetPaged(int page, int pageSize);
        Result Delete(int id, long requesterId);   // autor ili admin
        Result<TourReviewSummaryDto> SummaryByTour(long tourId);
    }
}
