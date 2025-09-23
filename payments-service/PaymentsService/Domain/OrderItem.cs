namespace PaymentsService.Domain
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public ShoppingCart Cart { get; set; } = default!;

        public long TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public long? AuthorId { get; set; }
        public OrderItem()
        {

        }
        public OrderItem(string tourName, decimal price, long tourId, long cartId)
        {

            TourName = tourName;
            Price = price;
            TourId = tourId;
            CartId = cartId;
        }


    }
}
