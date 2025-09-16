namespace ToursService.Domain.RepositoryInterfaces
{
    public interface ITourExecutionRepository
    {
        public new TourExecution? Get(long id);
        TourExecution Create(TourExecution execution);

        TourExecution Update(TourExecution execution);

        public void Delete(long id);
    }
}
