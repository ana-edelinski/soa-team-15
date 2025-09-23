namespace PaymentsService.Dtos
{
    public class ShoppingCartDto
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public List<OrderItemDto> Items { get; set; }
        //public List<TourPurchaseTokenDto> PurchaseTokens { get; set; }
        public decimal TotalPrice { get; set; }
        public ShoppingCartDto()
        {
            Items = new List<OrderItemDto>();
            //PurchaseTokens = new List<TourPurchaseTokenDto>();
            //CalculateTotalPrice();
        }

        //public void CalculateTotalPrice()
        //{
        //    TotalPrice = Items.Sum(item => item.Price);
        //}
    }
}
