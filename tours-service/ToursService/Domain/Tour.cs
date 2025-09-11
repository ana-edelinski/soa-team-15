using System.Security.Cryptography;

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


        public Tour(string name, string? description, string? difficulty, double price, List<TourTags> tags, long userId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid Name.");
            Name = name;
            Description = description;
            Difficulty = difficulty;
            if (tags == null || tags.Count == 0)
            { tags = new List<TourTags>(); }
            Tags = tags;
            if (userId <= 0)
                throw new ArgumentException("Invalid UserId. UserId must be a positive number.");
            UserId = userId;
            Status = TourStatus.Draft;
            Price = price;
            LengthInKm = 0;
            PublishedTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            ArchiveTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);


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

        public void UpdateLength(double length)
        {
            LengthInKm = length;
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

