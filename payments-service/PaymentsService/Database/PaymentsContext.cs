using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace PaymentsService.Database
{
    public class PaymentsContext : DbContext
    {
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public PaymentsContext(DbContextOptions<PaymentsContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<ShoppingCart>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();

                e.HasMany(x => x.Items)
                    .WithOne()
                    .HasForeignKey(x => x.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<OrderItem>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.TourName)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(x => x.Price)
                    .HasColumnType("numeric(10,2)")  
                    .IsRequired();

                e.Property(x => x.TourId).IsRequired();
                e.Property(x => x.CartId).IsRequired();
            });
        }
    }
}
