namespace PaymentsService.Dtos
{
    public class PurchaseTokenDto
    {
        public long TourId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
