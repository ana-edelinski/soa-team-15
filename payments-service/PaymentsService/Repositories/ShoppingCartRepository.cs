using PaymentsService.Database;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;

namespace PaymentsService.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly PaymentsContext _db;

        public ShoppingCartRepository(PaymentsContext db) => _db = db;

        public ShoppingCart Create(ShoppingCart shoppingCart)
        {
            _db.ShoppingCarts.Add(shoppingCart);
            _db.SaveChanges();
            return shoppingCart;
        }

        public List<ShoppingCart> GetAll(long userId)
        {
            return _db.ShoppingCarts
                .Where(cart => cart.UserId == userId)
                .ToList();
        }
    }
}
