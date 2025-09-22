namespace ToursService.Integrations
{

    
    public interface IPaymentClient
    {
        Task<List<long>> GetPurchasedIdsAsync(long userId, CancellationToken ct = default);
    }
}
