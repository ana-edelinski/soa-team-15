using Microsoft.EntityFrameworkCore;
using ToursService.Database;
using ToursService.Domain;
using ToursService.Domain.RepositoryInterfaces;

namespace ToursService.Repositories
{
    public class TourExecutionRepository : ITourExecutionRepository
    {
        private readonly ToursContext _dbContext;
        private readonly DbSet<TourExecution> _dbSet;

        public TourExecutionRepository(ToursContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TourExecution>();
        }

        public new TourExecution? Get(long id)
        {
            return _dbContext.TourExecution
        .Where(t => t.Id == id)
        .FirstOrDefault();

        }
        public TourExecution Update(TourExecution aggregateRoot)
        {
            _dbContext.Entry(aggregateRoot).State = EntityState.Modified;
            _dbContext.SaveChanges();
            return aggregateRoot;
        }

        public TourExecution Create(TourExecution execution)
        {
            _dbContext.TourExecution.Add(execution);
            _dbContext.SaveChanges();
            return execution;
        }

        public void Delete(long id)
        {
            var execution = _dbSet
                //.Include(t => t.CompletedKeys)
                .FirstOrDefault(t => t.Id == id);

            if (execution != null)
            {
                _dbSet.Remove(execution);
                _dbContext.SaveChanges();
            }
        }

        public TourExecution? GetByTourAndTourist(long touristId, long tourId)
        {
            return _dbContext.TourExecution
        .FirstOrDefault(t => t.TouristId == touristId && t.TourId == tourId);
        }

        public bool KeyPointExists(long keyPointId)
        {
            return _dbContext.KeyPoints.Any(kp => kp.Id == keyPointId);
        }

        public ICollection<KeyPoint> GetKeyPointsByTourId(long tourId)
        {
            var tour = _dbContext.Tours
                .Include(t => t.KeyPoints)
                .FirstOrDefault(t => t.Id == tourId);

            return tour?.KeyPoints.ToList() ?? new List<KeyPoint>();
        }

        public TourExecution? GetActiveTourByTourist(long touristId)
        {
            return _dbContext.TourExecution
                .FirstOrDefault(te => te.TouristId == touristId && te.Status == TourExecutionStatus.Active);
        }

        public bool CheckIfCompleted(long userId, long tourId)
        {
            var tourExecution = _dbContext.TourExecution
                .FirstOrDefault(te => te.TouristId == userId && te.TourId == tourId);

            return tourExecution != null && tourExecution.Status == TourExecutionStatus.Completed;

        }

        public List<long> FindAllCompletedForUser(long userId)
        {
            var results = _dbContext.TourExecution
            .Where(te => te.TouristId == userId && te.Status == TourExecutionStatus.Completed)
            .Select(te => te.TourId)
            .ToList();

            return results;

        }
    }
}
