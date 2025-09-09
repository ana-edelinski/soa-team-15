using Microsoft.EntityFrameworkCore;                  // 👈 dodaj ovo
using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;

namespace ToursService.Repositories
{
    public class TourReviewRepository : ITourReviewRepository
    {
        private readonly ToursContext _db;

        public TourReviewRepository(ToursContext db) => _db = db;

        public TourReview Create(TourReview review)
        {
            _db.TourReviews.Add(review);
            _db.SaveChanges();
            return review;
        }

        public TourReview? GetById(int id)
        {
            return _db.TourReviews
                      .Include(r => r.Images)
                      .FirstOrDefault(r => r.Id == id);
        }

        public List<TourReview> GetByTour(long tourId)
        {
            return _db.TourReviews
                      .Include(r => r.Images)
                      .Where(r => r.IdTour == tourId)
                      .OrderBy(r => r.Id)
                      .AsNoTracking()
                      .ToList();
        }

        public List<TourReview> GetByTourist(long touristId)
        {
            return _db.TourReviews
                      .Include(r => r.Images)
                      .Where(r => r.IdTourist == touristId)
                      .OrderBy(r => r.Id)
                      .AsNoTracking()
                      .ToList();
        }

        public PagedResult<TourReview> GetPaged(int page, int pageSize)
        {
            var query = _db.TourReviews
                           .Include(r => r.Images)
                           .AsNoTracking();

            var totalCount = query.Count();

            var items = query
                .OrderBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<TourReview>(items, totalCount);
        }

        public void Update(TourReview review)
        {
            _db.TourReviews.Update(review);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _db.TourReviews.FirstOrDefault(r => r.Id == id);
            if (entity == null) return;

            _db.TourReviews.Remove(entity);
            _db.SaveChanges();
        }
    }
}
