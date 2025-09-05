namespace ToursService.Domain
{
    public class TourReview
    {
        public long Id { get; set; }
        public long IdTour { get; private set; }
        public long IdTourist { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public DateTime? DateTour { get; private set; }
        public DateTime? DateComment { get; set; }
        public ICollection<TourReviewImage> Images { get; private set; } = new List<TourReviewImage>();


        public TourReview(long idTour, long idTourist, int rating, string comment, DateTime? dateTour, DateTime? dateComment)
        {
            IdTour = idTour != 0 ? idTour : throw new ArgumentException("Invalid idTour");
            IdTourist = idTourist != 0 ? idTourist : throw new ArgumentException("Invalid idTourist");
            Rating = rating >= 1 && rating <= 5 ? rating : throw new ArgumentException("Invalid Rating");
            Comment = string.IsNullOrWhiteSpace(comment) ? "" : comment;
            DateTour = dateTour != null ? dateTour : throw new ArgumentNullException("Invalid Date");
            DateComment = dateComment != null ? dateComment : throw new ArgumentNullException("Invalid Date");
        }

        public TourReview()
        {

        }
    }
}

