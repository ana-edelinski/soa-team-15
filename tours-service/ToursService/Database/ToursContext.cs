using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Emit;
using ToursService.Domain;

namespace ToursService.Database
{
    public class ToursContext : DbContext
    {
        public DbSet<Tour> Tours { get; set; }
        public DbSet<KeyPoint> KeyPoints { get; set; }
        public DbSet<TourReview> TourReviews { get; set; }
        public DbSet<TourReviewImage> TourReviewImages { get; set; }
        public DbSet<TourTransportTime> TourTransportTimes { get; set; }

        public DbSet<Position> Positions { get; set; }
        public DbSet<TourExecution> TourExecution { get; set; }

        public ToursContext(DbContextOptions<ToursContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            
            // TOUR
            b.Entity<Tour>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Difficulty).HasMaxLength(50);
                e.Property(x => x.Price).HasColumnType("numeric(10,2)");
                e.Property(x => x.LengthInKm);


                e.Property(x => x.Tags)
                    .HasColumnType("integer[]");   // List<TourTags> (enum) -> int[] (integer[])

                // 1:N Tour -> KeyPoints
                e.HasMany(x => x.KeyPoints)
                 .WithOne()
                 .HasForeignKey(k => k.TourId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.UserId, x.Status });
                e.HasIndex(x => x.PublishedTime);

                // 1:N Tour -> TransportTimes
                e.HasMany(t => t.TransportTimes)
                .WithOne()
                .HasForeignKey(tt => tt.TourId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            // KEYPOINT
            b.Entity<KeyPoint>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).IsRequired().HasMaxLength(2000);
                e.Property(x => x.Image).IsRequired().HasMaxLength(500);
                e.Property(x => x.Longitude);
                e.Property(x => x.Latitude);
            });

            // REVIEW
            b.Entity<TourReview>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.IdTour, x.IdTourist });
                // jednostavan check za ocenu 1-5
                e.ToTable(t => t.HasCheckConstraint("CK_TourReview_Rating_1_5", "\"Rating\" >= 1 AND \"Rating\" <= 5"));
            });

            b.Entity<TourReviewImage>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Url).IsRequired().HasMaxLength(500);
                e.HasOne(x => x.Review)
                 .WithMany(r => r.Images)
                 .HasForeignKey(x => x.ReviewId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            

            //POSITION
            b.Entity<Position>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Latitude).IsRequired();
                e.Property(x => x.Longitude).IsRequired();

                // jedan turista = jedna pozicija
                e.HasIndex(x => x.TouristId).IsUnique();
            });


            b.Entity<TourExecution>().Property(item => item.CompletedKeys).HasColumnType("jsonb"); //value object cuva kao json
            // TOUR EXECUTION
            b.Entity<TourExecution>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.TourId).IsRequired();
                e.Property(x => x.TouristId).IsRequired();
                e.Property(x => x.LocationId).IsRequired();

                e.Property(x => x.LastActivity);
                e.Property(x => x.Status)
                    .HasConversion<string>()   
                    .IsRequired()
                    .HasMaxLength(20);

                e.HasIndex(x => x.TouristId);
                e.HasIndex(x => new { x.TourId, x.TouristId });
            });

            b.Entity<TourExecution>()
            .HasOne<Position>()
            .WithMany()
            .HasForeignKey(s => s.LocationId);

            // TOUR TRANSPORT TIME
            b.Entity<TourTransportTime>(e =>
            {
                e.ToTable("TourTransportTimes");         
                e.HasKey(x => x.Id);

                e.Property(x => x.TourId).IsRequired();

                e.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

                e.Property(x => x.Minutes)
                .IsRequired();

                e.HasIndex(x => new { x.TourId, x.Type }).IsUnique();

                e.ToTable(t => t.HasCheckConstraint("CK_TourTransportTime_Minutes_Positive", "\"Minutes\" > 0"));
            });


        }
    }
}

