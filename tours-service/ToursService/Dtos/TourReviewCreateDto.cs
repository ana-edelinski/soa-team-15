namespace ToursService.Dtos
{
    public class TourReviewCreateDto
    {
        public long IdTour { get; set; }
        public long IdTourist { get; set; }
        public int Rating { get; set; }              // 1..5
        public string? Comment { get; set; }
        public DateTime? DateTour { get; set; }      // datum kada je posećena tura
        public string? Image { get; set; }           // za sada jedan URL
    }
}
