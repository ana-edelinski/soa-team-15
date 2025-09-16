namespace ToursService.Dtos
{
    public class TourExecutionDto
    {
        public long Id { get; set; }
        public long TourId { get; set; }
        public long TouristId { get; set; }

        public long LocationId { get; set; } 

        public DateTime? LastActivity { get; set; }
        public TourExecutionStatus Status { get; set; }

        public TourExecutionDto() { }

        public TourExecutionDto(long id, long tourId, long touristId, long locationId, DateTime? lastActivity, TourExecutionStatus status)
        {
            Id = id;
            TourId = tourId;
            TouristId = touristId;
            LocationId = locationId;
            LastActivity = lastActivity;
            Status = status;
        }
    }

    public enum TourExecutionStatus
    {
        Active,
        Completed,
        Abandoned
    }
}
