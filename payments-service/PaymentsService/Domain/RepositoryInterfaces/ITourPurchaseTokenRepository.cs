namespace PaymentsService.Domain.RepositoryInterfaces
{
    public interface ITourPurchaseTokenRepository
    {
        void AddRange(IEnumerable<TourPurchaseToken> tokens);
        bool Exists(long userId, long tourId);
        List<long> GetPurchasedTourIds(long userId);
        void SaveChanges();
    }
}
