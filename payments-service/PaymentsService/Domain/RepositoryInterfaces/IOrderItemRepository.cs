namespace PaymentsService.Domain.RepositoryInterfaces
{
    public interface IOrderItemRepository
    {
        OrderItem Create(OrderItem item);
        bool Delete(int itemId);
        OrderItem? Get(int itemId);
        List<OrderItem> GetAll(long cartId);
        decimal CalculateTotalPrice(long itemId);

        void RemoveByCart(long cartId);
        void SaveChanges();

    }
}
