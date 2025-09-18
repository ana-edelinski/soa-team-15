namespace ToursService.Domain
{
    public enum TransportType
    {
        Walk = 0,  
        Bike = 1,   
        Car  = 2  
    }

    public class TourTransportTime
    {
        public long Id { get; private set; }
        public long TourId { get; private set; }   
        public TransportType Type { get; private set; }
        public int Minutes { get; private set; }     

        private TourTransportTime() { } 

        public TourTransportTime(TransportType type, int minutes)
        {
            if (minutes <= 0) throw new ArgumentException("Minutes must be > 0.");
            Type = type;
            Minutes = minutes;
        }

        public void Update(int minutes)
        {
            if (minutes <= 0) throw new ArgumentException("Minutes must be > 0.");
            Minutes = minutes;
        }
    }
}
