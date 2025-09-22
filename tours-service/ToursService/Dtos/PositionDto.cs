namespace ToursService.Dtos
{
    public class PositionDto
    {
        public long Id { get; set; }
        public long TouristId { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public PositionDto() { }
    }
}
