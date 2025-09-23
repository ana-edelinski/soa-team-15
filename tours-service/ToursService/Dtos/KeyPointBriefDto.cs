using ToursService.Domain;

namespace ToursService.Dtos
{
    public class KeyPointBriefDto
    {
        public string Title { get; set; } = default!;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
