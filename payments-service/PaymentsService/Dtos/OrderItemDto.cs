namespace PaymentsService.Dtos
{
    public class OrderItemDto
    {
        public long Id { get; set; }
        public string TourName { get; set; }
        public decimal Price { get; set; }
        public long TourId { get; set; }
        public long CartId { get; set; }
    }
}
