namespace PaymentsService.Domain.RepositoryInterfaces
{
    public interface ITourPurchaseTokenRepository
    {
        void AddRange(IEnumerable<TourPurchaseToken> tokens);
        bool Exists(long userId, long tourId);
        List<long> GetPurchasedTourIds(long userId);
        void SaveChanges();

        //SAGA
        Task<TourPurchaseToken?> GetAvailableAsync(long userId, long tourId, CancellationToken ct);
        Task<TourPurchaseToken?> GetByExecutionIdAsync(long executionId, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);

    }
}
