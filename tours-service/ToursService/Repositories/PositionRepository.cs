using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;

namespace ToursService.Repositories
{
    public class PositionRepository: IPositionRepository
    {
        private readonly ToursContext _dbContext;

        public PositionRepository(ToursContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Position? GetByTouristId(long touristId)
        {
            return _dbContext.Positions
                .FirstOrDefault(p => p.TouristId == touristId);
        }

        public void Update(Position position)
        {
            var existing = _dbContext.Positions
                .FirstOrDefault(p => p.TouristId == position.TouristId);

            if (existing == null)
            {
                // prvi put – ubaci
                _dbContext.Positions.Add(position);
            }
            else
            {
                // već postoji – update polja
                existing.Update(position.Latitude, position.Longitude);
                _dbContext.Positions.Update(existing);
            }

            _dbContext.SaveChanges();
        }
    }
}
