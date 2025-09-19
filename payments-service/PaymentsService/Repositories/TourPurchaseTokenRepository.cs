using PaymentsService.Database;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;

namespace PaymentsService.Repositories
{
    public class TourPurchaseTokenRepository : ITourPurchaseTokenRepository
    {
        private readonly PaymentsContext _db;
        public TourPurchaseTokenRepository(PaymentsContext db) => _db = db;

        public void AddRange(IEnumerable<TourPurchaseToken> tokens)
        {
            _db.TourPurchaseTokens.AddRange(tokens);
        }

        public bool Exists(long userId, long tourId)
        {
            return _db.TourPurchaseTokens.Any(t => t.UserId == userId && t.TourId == tourId);
        }

        public List<long> GetPurchasedTourIds(long userId)
        {
            return _db.TourPurchaseTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.TourId)
                .ToList();
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
