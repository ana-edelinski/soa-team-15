using System.Security.Cryptography;
using System.Linq;   
namespace ToursService.Domain
{
    public class Tour
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }

        public string? Difficulty { get; private set; }

        public List<TourTags> Tags { get; private set; } = new List<TourTags>();
        public TourStatus Status { get; private set; }
        public double Price { get; private set; }
        public long UserId { get; private set; }

        public double LengthInKm { get; private set; }

        public DateTime PublishedTime { get; private set; }

        public DateTime? ArchiveTime { get; private set; }


        public ICollection<KeyPoint> KeyPoints { get; private set; } = new List<KeyPoint>();
        public ICollection<TourTransportTime> TransportTimes { get; private set; } = new List<TourTransportTime>();

       



        public Tour(string name, string? description, string? difficulty, double price, List<TourTags> tags, long userId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid Name.");
            Name = name;
            Description = description;
            Difficulty = difficulty;
            Tags = tags ?? new List<TourTags>();
            if (userId <= 0) throw new ArgumentException("Invalid UserId.");
            UserId = userId;

            Status = TourStatus.Draft;
            Price = price;

            // KLJUČNO: kolona je NOT NULL → NIKAD null
            PublishedTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

            // Ovo je nullable, smije biti null
            ArchiveTime = null;
        }



        //TODO: kada se obrise tura, pozziva se i brisanje ovoga
        public bool RemoveTransportTime(long authorId, TransportType type)
        {
            IsAuthor(authorId);
            var existing = TransportTimes.FirstOrDefault(t => t.Type == type);
            if (existing is null) return false;
            TransportTimes.Remove(existing);
            return true;
        }

        public void Archive(long authorId)
        {
            if (Status != TourStatus.Published) throw new ArgumentException("Tour must be published in order to be archived");
            IsAuthor(authorId);

            ArchiveTime = DateTime.UtcNow;
            Status = TourStatus.Archived;
        }

        private void IsAuthor(long userId)
        {
            if (UserId != userId) throw new UnauthorizedAccessException("User is not the author of the tour");
        }

        public bool Reactivate(long authorId)
        {
            if (Status != TourStatus.Archived)
            {
                throw new ArgumentException("Tour must be archived in order to be reactivated");
            }

            IsAuthor(authorId);

            Status = TourStatus.Published;

            ArchiveTime = null;

            return true;
        }

        public void Publish(long authorId)
            {
                if (Status != TourStatus.Draft)
                    throw new ArgumentException("Only draft tours can be published.");

                IsAuthor(authorId);

                Status = TourStatus.Published;
                PublishedTime = DateTime.UtcNow;
                ArchiveTime = null;
            }

        public void UpdateLength(double length)
        {
            LengthInKm = length;
        }

         public void AddOrUpdateTransportTime(long authorId, TransportType type, int minutes)
        {
            IsAuthor(authorId);
            if (minutes <= 0) throw new ArgumentException("Duration must be > 0 minutes.");

            var existing = TransportTimes.FirstOrDefault(t => t.Type == type);
            if (existing is null) TransportTimes.Add(new TourTransportTime(type, minutes));
            else existing.Update(minutes);
        }

            public double RecalculateLength(List<KeyPoint> keyPoints)
            {
                if (keyPoints == null || keyPoints.Count < 2)
                {
                    return 20;
                }

                double total = 0;

                var ordered = keyPoints.OrderBy(kp => kp.Id).ToList();

                for (int i = 1; i < ordered.Count; i++)
                {
                    var a = ordered[i - 1];
                    var b = ordered[i];
                    total += GeoDistance.HaversineKm(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
                }

                return Math.Round(total, 3);
            }



    }

    
    public enum TourStatus
    {
        Draft,
        Published,
        Archived
    }

    public enum TourTags
    {
        Cycling,
        Culture,
        Adventure,
        FamilyFriendly,
        Nature,
        CityTour,
        Historical,
        Relaxation,
        Wildlife,
        NightTour,
        Beach,
        Mountains,
        Photography,
        Guided,
        SelfGuided
    }
}

