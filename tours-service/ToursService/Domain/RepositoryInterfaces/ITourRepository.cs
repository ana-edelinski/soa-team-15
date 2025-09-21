namespace ToursService.Domain.RepositoryInterfaces
{
    public interface ITourRepository
    {
        Tour Create(Tour tour);
        Tour? GetById(long id);
        List<Tour> GetAll();
        List<Tour> GetByAuthor(long userId);

        PagedResult<Tour> GetPaged(int page, int pageSize);

        void Update(Tour tour);
        List<Tour> GetPublished();

        void RemoveTransportTime(TourTransportTime transportTime);

    }
}
