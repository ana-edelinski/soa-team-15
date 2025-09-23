using Microsoft.EntityFrameworkCore;
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
                .Include(c => c.Items)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        // NEW:
        public ShoppingCart? GetById(long cartId)
        {
            return _db.ShoppingCarts
                     .Include(c => c.Items)
                     .FirstOrDefault(c => c.Id == cartId);
        }

        public void Update(ShoppingCart cart)
        {
            _db.ShoppingCarts.Update(cart);
        }

        public void SaveChanges() => _db.SaveChanges();

        public void ClearItems(long cartId)
        {
            var cart = _db.ShoppingCarts
                          .Include(c => c.Items)
                          .FirstOrDefault(c => c.Id == cartId);

            if (cart == null) return;

            _db.OrderItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.TotalPrice = 0m;
            _db.SaveChanges();
        }
    }
}
