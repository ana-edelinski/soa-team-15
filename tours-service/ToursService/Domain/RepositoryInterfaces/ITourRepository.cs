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
        void Delete(long id);
        List<Tour> GetPublished();
        public Task<List<Tour>> GetByIdsAsync(IEnumerable<long> ids);


    }
}
