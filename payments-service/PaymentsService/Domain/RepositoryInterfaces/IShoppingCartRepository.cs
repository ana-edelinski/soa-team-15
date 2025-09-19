namespace PaymentsService.Domain.RepositoryInterfaces
{
    public interface IShoppingCartRepository
    {
        List<ShoppingCart> GetAll(long userId);
        ShoppingCart Create(ShoppingCart entity);

        ShoppingCart? GetById(long cartId);
        void Update(ShoppingCart cart);
        void SaveChanges();
        public void ClearItems(long cartId);

    }
}
