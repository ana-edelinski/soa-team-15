using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain;

namespace PaymentsService.Database
{
    public class PaymentsContext : DbContext
    {
        public PaymentsContext(DbContextOptions<PaymentsContext> options) : base(options) { }

        public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<TourPurchaseToken> TourPurchaseTokens => Set<TourPurchaseToken>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<ShoppingCart>(e =>
            {
                e.ToTable("shopping_carts");
                e.HasKey(x => x.Id);
                e.Property(x => x.TotalPrice).HasColumnType("numeric(12,2)");
                e.HasMany(x => x.Items)
                    .WithOne(x => x.Cart)
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<OrderItem>(e =>
            {
                e.ToTable("order_items");
                e.HasKey(x => x.Id);
                e.Property(x => x.Price).HasColumnType("numeric(12,2)");
                e.Property(x => x.TourName).HasMaxLength(200);
            });

            mb.Entity<TourPurchaseToken>(e =>
            {
                e.ToTable("tour_purchase_tokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Token).HasMaxLength(128);
                e.HasIndex(x => new { x.UserId, x.TourId }).IsUnique();
                e.Property(x => x.Status).HasMaxLength(16);
                e.Property(x => x.LockedBy).HasColumnType("uuid").IsRequired(false);
            });
        }
    }
}
