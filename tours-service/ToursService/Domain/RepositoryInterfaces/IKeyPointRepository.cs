namespace ToursService.Domain.RepositoryInterfaces
{
    public interface IKeyPointRepository
    {
        KeyPoint Create(KeyPoint keyPoint);
        KeyPoint? GetById(int id);
        List<KeyPoint> GetByTour(long tourId);

        PagedResult<KeyPoint> GetPaged(int page, int pageSize);

        void Update(KeyPoint keyPoint);
        void Delete(int id);
    }
}
