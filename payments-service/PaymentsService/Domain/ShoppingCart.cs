namespace PaymentsService.Domain
{
    public class ShoppingCart
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<OrderItem> Items { get; private set; }
        //public List<TourPurchaseToken> PurchaseTokens { get; private set; }

        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ShoppingCart()
        {
            Items = new List<OrderItem>();
            //PurchaseTokens = new List<TourPurchaseToken>();
        }
    }
}
