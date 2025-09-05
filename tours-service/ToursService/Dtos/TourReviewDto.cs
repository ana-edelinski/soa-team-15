namespace ToursService.Dtos
{
    public class TourReviewDto
    {
        public long Id { get; set; }
        public long IdTour { get; set; }
        public long IdTourist { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? DateTour { get; set; }
        public DateTime? DateComment { get; set; }   // serverski setovano
        public string? Image { get; set; }
    }
}
