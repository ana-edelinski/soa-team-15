using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Domain;
using PaymentsService.Domain.RepositoryInterfaces;

namespace PaymentsService.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly PaymentsContext _db;
        public OrderItemRepository(PaymentsContext db) => _db = db;

        public decimal CalculateTotalPrice(long itemId)
        {

            var item = _db.OrderItems
                .FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return -1;
            }

            return item.Price;
        }

        public OrderItem Create(OrderItem item)
        {
            _db.OrderItems.Add(item);
            _db.SaveChanges();
            return item;
        }

        public bool Delete(int itemId)
        {
            var existing = _db.OrderItems.FirstOrDefault(x => x.Id == itemId);
            if (existing == null) return false;

            _db.OrderItems.Remove(existing);
            _db.SaveChanges();
            return true;
        }

        public OrderItem? Get(int itemId)
        {
            return _db.OrderItems.AsNoTracking().FirstOrDefault(x => x.Id == itemId);
        }

        public List<OrderItem> GetAll(long cartId)
        {
            var orderItems = _db.OrderItems.ToList();
            var result = orderItems.Where(item => item.CartId == cartId).ToList();

            return result;
            //return Result.Ok(result);

        }
    }
}
