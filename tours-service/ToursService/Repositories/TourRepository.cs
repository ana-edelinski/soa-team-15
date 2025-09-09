using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;

namespace ToursService.Repositories
{
    public class TourRepository : ITourRepository
    {
        private readonly ToursContext _db;

        public TourRepository(ToursContext db) => _db = db;

        public Tour Create(Tour tour)
        {
            _db.Tours.Add(tour);
            _db.SaveChanges();
            return tour;
        }

        public Tour? GetById(long id)
        {
            return _db.Tours
                      .FirstOrDefault(t => t.Id == id);
        }

        public List<Tour> GetAll()
        {
            return _db.Tours
                      .OrderBy(t => t.Id)
                      .ToList();
        }

        public List<Tour> GetByAuthor(long userId)
        {
            return _db.Tours
                      .Where(t => t.UserId == userId)
                      .OrderBy(t => t.Id)
                      .ToList();
        }

        public PagedResult<Tour> GetPaged(int page, int pageSize)
        {
            var query = _db.Tours.AsQueryable();

            var totalCount = query.Count();

            var items = query
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Tour>(items, totalCount);
        }

        public void Update(Tour tour)
        {
            _db.Tours.Update(tour);
            _db.SaveChanges();
        }

        public void Delete(long id)
        {
            var entity = _db.Tours.FirstOrDefault(t => t.Id == id);
            if (entity == null) return;

            _db.Tours.Remove(entity);
            _db.SaveChanges();
        }

        public List<Tour> GetPublished()
        {
            return _db.Tours
                      .Where(t => t.Status == TourStatus.Published)
                      .OrderByDescending(t => t.PublishedTime)
                      .ToList();
        }
    }
}
