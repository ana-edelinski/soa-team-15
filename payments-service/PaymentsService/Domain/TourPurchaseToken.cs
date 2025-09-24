namespace PaymentsService.Domain
{
    public class TourPurchaseToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }      // kupac
        public long TourId { get; set; }      // kupljena tura
        public string Token { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        //POLJA ZA SAGU
        public string Status { get; set; } = "Available"; // Available | Locked | Consumed
        public Guid? LockedBy { get; set; }               // SagaId koja drži lock
        public DateTime? LockedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }          // TTL za lock, npr. +2 min
        public long? ExecutionId { get; set; }
    }
}
