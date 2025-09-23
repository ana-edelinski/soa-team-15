namespace PaymentsService.Domain
{
    public class TourPurchaseToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }      // kupac
        public long TourId { get; set; }      // kupljena tura
        public string Token { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    }
}
