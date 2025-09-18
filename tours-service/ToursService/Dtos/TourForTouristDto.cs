using ToursService.Domain;

namespace ToursService.Dtos
{
    public class TourForTouristDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public List<TourTags> Tags { get; set; }
        public double Price { get; set; }
        public long UserId { get; set; }
        public ICollection<KeyPointDto> KeyPoints { get; set; } = new List<KeyPointDto>();

        public TourForTouristDto() { }

        public TourForTouristDto(Tour tour, IEnumerable<KeyPoint> keyPoints)
        {
            Id = tour.Id;
            Name = tour.Name;
            Description = tour.Description;
            Difficulty = tour.Difficulty;
            Tags = (tour.Tags ?? new List<ToursService.Domain.TourTags>())
            .Select(t => (ToursService.Dtos.TourTags)(int)t)   // double-cast domain→int→dto
            .ToList();
            Price = tour.Price;
            UserId = tour.UserId;
            KeyPoints = keyPoints.Select(k => new KeyPointDto(k)).ToList();
        }

        
    }
}
