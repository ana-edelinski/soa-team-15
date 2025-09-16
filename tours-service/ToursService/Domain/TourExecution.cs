namespace ToursService.Domain
{
    public class TourExecution 
    {
        public long Id { get; private set; }
        public long TourId { get; private set; }
        public long TouristId { get; private set; }

        public long LocationId { get; private set; } 

        public DateTime? LastActivity { get; private set; }
        public TourExecutionStatus Status { get; private set; }

        //public List<CompletedKeyPoint> CompletedKeys { get; private set; } //ovde se nalazi endTime za svaku kt

        public TourExecution(long tourId, long touristId, long locationId, DateTime? lastActivity, TourExecutionStatus status/*, List<CompletedKeyPoint> completedKeys*/)
        {
            TourId = tourId;
            TouristId = touristId;
            LocationId = locationId;
            LastActivity = lastActivity;
            Status = status;
            if (!DateTime.TryParse(LastActivity.ToString(), out _)) lastActivity = DateTime.UtcNow;

            //CompletedKeys = completedKeys;
        }

        public void StartTourExecution()
        {
            LastActivity = DateTime.UtcNow;
            Status = TourExecutionStatus.Active;
        }

        public void CompleteTourExecution()
        {
            if (Status != TourExecutionStatus.Active)
                throw new ArgumentException("Invalid end status.");

            Status = TourExecutionStatus.Completed;
            LastActivity = DateTime.UtcNow;
        }

        public void AbandonTourExecution()
        {
            if (Status != TourExecutionStatus.Active)
                throw new ArgumentException("Invalid end status.");

            Status = TourExecutionStatus.Abandoned;
            LastActivity = DateTime.UtcNow;
        }

        public void UpdateLastActivity()
        {
            LastActivity = DateTime.UtcNow;
        }


        //public CompletedKeyPoint CompleteKeyPoint(long keyPointId)
        //{
        //    ValidateTourExecutionActive();
        //    ValidateKeyPointNotCompleted(keyPointId);

        //    var completedKeyPoint = new CompletedKeyPoint(keyPointId, DateTime.UtcNow);
        //    CompletedKeys.Add(completedKeyPoint);
        //    LastActivity = DateTime.UtcNow;
        //    return completedKeyPoint;
        //}

        private void ValidateTourExecutionActive()
        {
            if (Status != TourExecutionStatus.Active) throw new ArgumentException("Tour is not active.");
        }

        //private void ValidateKeyPointNotCompleted(long keyPointId)
        //{
        //    if (CompletedKeys.Any(ckp => ckp.KeyPointId == keyPointId)) throw new ArgumentException($"Key point with ID {keyPointId} is already completed.");
        //}
    }

    public enum TourExecutionStatus
    {
        Active,
        Completed,
        Abandoned
    }
}
