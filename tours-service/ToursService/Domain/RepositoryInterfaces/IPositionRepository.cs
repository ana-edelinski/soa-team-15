namespace ToursService.Domain.RepositoryInterfaces
{
    public interface IPositionRepository
    {
        Position? GetByTouristId(long touristId);
        void Update(Position position);
    }
}
