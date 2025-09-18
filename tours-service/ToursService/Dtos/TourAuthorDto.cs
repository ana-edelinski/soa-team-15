using ToursService.Domain;

namespace ToursService.Dtos
{
    public class TourAuthorDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Difficulty { get; set; }
    public List<TourTags> Tags { get; set; } = new();

    public TourStatus Status { get; set; }
    public double Price { get; set; }
    public long UserId { get; set; }

    public double LengthInKm { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public List<KeyPointDto> KeyPoints { get; set; } = new();
    public List<TourTransportTimeDto> TransportTimes { get; set; } = new();
}
}
