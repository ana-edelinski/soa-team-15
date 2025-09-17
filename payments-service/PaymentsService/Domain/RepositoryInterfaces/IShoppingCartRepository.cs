namespace PaymentsService.Domain.RepositoryInterfaces
{
    public interface IShoppingCartRepository
    {
        List<ShoppingCart> GetAll(long userId);
        ShoppingCart Create(ShoppingCart entity);
    }
}
