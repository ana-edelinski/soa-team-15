namespace ToursService.Dtos
{
    public class TourPublicDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Difficulty { get; set; }
    public List<TourTags> Tags { get; set; } = new();

    public double Price { get; set; }
    public double LengthInKm { get; set; }
    public DateTime? PublishedAt { get; set; }

    public KeyPointDto? FirstKeyPoint { get; set; } 
}
}
