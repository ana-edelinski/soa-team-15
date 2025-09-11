namespace ToursService.Domain.RepositoryInterfaces
{
    public interface ITourReviewRepository
    {
        TourReview Create(TourReview review);
        TourReview? GetById(int id);
        List<TourReview> GetByTour(long tourId);
        List<TourReview> GetByTourist(long touristId);

        PagedResult<TourReview> GetPaged(int page, int pageSize);

        void Update(TourReview review);
        void Delete(int id);
    }
}
