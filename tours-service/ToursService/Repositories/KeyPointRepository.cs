using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;

namespace ToursService.Repositories
{
    public class KeyPointRepository: IKeyPointRepository
    {
        private readonly ToursContext _db;

        public KeyPointRepository(ToursContext db) => _db = db;

        public KeyPoint Create(KeyPoint keyPoint)
        {
            _db.KeyPoints.Add(keyPoint);
            _db.SaveChanges();
            return keyPoint;
        }

        public KeyPoint? GetById(int id)
        {
            return _db.KeyPoints.FirstOrDefault(k => k.Id == id);
        }

        public List<KeyPoint> GetByTour(long tourId)
        {
            return _db.KeyPoints
                      .Where(k => k.TourId == tourId)
                      .OrderBy(k => k.Id)
                      .ToList();
        }

        public PagedResult<KeyPoint> GetPaged(int page, int pageSize)
        {
            var query = _db.KeyPoints.AsQueryable();

            var totalCount = query.Count();

            var items = query
                .OrderBy(k => k.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<KeyPoint>(items, totalCount);
        }

        public void Update(KeyPoint keyPoint)
        {
            _db.KeyPoints.Update(keyPoint);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _db.KeyPoints.FirstOrDefault(k => k.Id == id);
            if (entity == null) return;

            _db.KeyPoints.Remove(entity);
            _db.SaveChanges();
        }
    }
}
