namespace ToursService.Dtos
{
    public class TourDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public List<TourTags> Tags { get; set; } = new();
        public TourStatus Status { get; set; }
        public double Price { get; set; }
        public long UserId { get; set; }

        //public DateTime PublishedTime { get; set; }
        //public DateTime ArchiveTime { get; set; }
        //public ICollection<KeyPointDto> KeyPoints { get; set; } = new List<KeyPointDto>();

        public DateTime? PublishedTime { get; set; }
        public DateTime? ArchiveTime { get; set; }
        // 👇 NOVO — ovo traži tvoj front
        public double LengthInKm { get; set; }
        public int? DurationMinutes { get; set; }
        public string? StartPointName { get; set; }
        public List<string> PreviewImages { get; set; } = new();


        public List<KeyPointBriefDto>? KeyPoints { get; set; }


        public TourDto() { }


        public TourDto(long id, string name, string? description, string? difficulty,
                       List<TourTags> tags, long userId, TourStatus status, double price, double lengthInKm)
        {
            Id = id;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid Name.");
            Name = name;
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Invalid description.");
            Description = description;
            if (string.IsNullOrWhiteSpace(difficulty)) throw new ArgumentException("Invalid difficulty.");
            Difficulty = difficulty;
            Tags = tags ?? new List<TourTags>();
            if (userId <= 0) throw new ArgumentException("Invalid UserId.");
            UserId = userId;
            Status = status;
            Price = price;

           // LengthInKm = lengthInKm;
            //PublishedTime = publishedTime;
            //ArchiveTime = archivedTime;


        }

        public TourDto(long id, string name, string? description, string? difficulty, List<TourTags> tags, long userId, TourStatus status, double price, double discountedPrice, double lengthInKm, DateTime publishedTime, DateTime archivedTime, List<long> equipmentIds, List<long> keyPointIds)

        {
            Id = id;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid Name.");
            Name = name;
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Invalid description.");
            Description = description;
            if (string.IsNullOrWhiteSpace(difficulty)) throw new ArgumentException("Invalid difficulty.");
            Difficulty = difficulty;
            if (tags == null || tags.Count == 0)
            { tags = new List<TourTags>(); }
            Tags = tags;
            if (userId <= 0)
                throw new ArgumentException("Invalid UserId. UserId must be a positive number.");
            UserId = userId;

            Status = status;
            Price = price;
            //DiscountPrice = discountedPrice;
            LengthInKm = lengthInKm;
            //PublishedTime = publishedTime;
            //ArchiveTime = archivedTime;


        }





        public TourDto(
            long id, string name, string? description, string? difficulty,
            List<TourTags> tags, long userId, TourStatus status, double price,
            double lengthInKm, DateTime? publishedTime, DateTime? archiveTime)
        {
            Id = id;
            Name = name ?? string.Empty;
            Description = description;
            Difficulty = difficulty;
            Tags = tags ?? new();
            UserId = userId;
            Status = status;
            Price = price;
            LengthInKm = lengthInKm;
            PublishedTime = publishedTime;   // nullable → nullable
            ArchiveTime   = archiveTime;     // nullable → nullable
        }
    }

    public enum TourStatus { Draft, Published, Archived }
    public enum TourTags
    {
        Cycling, Culture, Adventure, FamilyFriendly, Nature, CityTour, Historical, Relaxation,
        Wildlife, NightTour, Beach, Mountains, Photography, Guided, SelfGuided
    }
}
