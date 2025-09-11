namespace ToursService.Domain
{
    public class Position
    {
        public int Id { get; set; }
        public long TouristId { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public Position(long touristId, double latitude, double longitude)
        {
            if (touristId <= 0)
                throw new ArgumentException("Invalid tourist id.");

            ValidateCoordinates(latitude, longitude);

            TouristId = touristId;
            Latitude = latitude;
            Longitude = longitude;
        }

        public void Update(double latitude, double longitude)
        {
            ValidateCoordinates(latitude, longitude);

            Latitude = latitude;
            Longitude = longitude;
        }

        private void ValidateCoordinates(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90.");
            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180.");
        }
    }
}

