namespace ToursService.Domain.RepositoryInterfaces
{
    public interface ITourExecutionRepository
    {
        public new TourExecution? Get(long id);
        TourExecution Create(TourExecution execution);

        TourExecution Update(TourExecution execution);

        public void Delete(long id);
        TourExecution? GetByTourAndTourist(long touristId, long tourId);
        public TourExecution? GetActiveTourByTourist(long touristId);

        public bool KeyPointExists(long keyPointId);
        public ICollection<KeyPoint> GetKeyPointsByTourId(long tourId);
        bool CheckIfCompleted(long userId, long tourId);
        List<long> FindAllCompletedForUser(long userId);


    }
}
