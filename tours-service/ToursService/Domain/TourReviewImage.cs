namespace ToursService.Domain
{
    public class TourReviewImage
    {
        public long Id { get; set; }
        public long ReviewId { get; set; }
        public string Url { get; set; } = string.Empty;
        public TourReview? Review { get; set; }
    }
}
