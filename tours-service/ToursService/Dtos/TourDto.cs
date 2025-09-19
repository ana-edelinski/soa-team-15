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

        // 👇 NOVO — ovo traži tvoj front
        public double LengthInKm { get; set; }
        public int? DurationMinutes { get; set; }
        public string? StartPointName { get; set; }
        public List<string> PreviewImages { get; set; } = new();

        public TourDto() { }

        public TourDto(long id, string name, string? description, string? difficulty,
                       List<TourTags> tags, long userId, TourStatus status, double price)
        {
            Id = id;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid Name.");
            Name = name;
            Description = description;
            Difficulty = difficulty;
            Tags = tags ?? new List<TourTags>();
            if (userId <= 0) throw new ArgumentException("Invalid UserId.");
            UserId = userId;
            Status = status;
            Price = price;
        }
    }

    public enum TourStatus { Draft, Published, Archived }
    public enum TourTags
    {
        Cycling, Culture, Adventure, FamilyFriendly, Nature, CityTour, Historical, Relaxation,
        Wildlife, NightTour, Beach, Mountains, Photography, Guided, SelfGuided
    }
}
