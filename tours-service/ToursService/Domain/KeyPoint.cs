namespace ToursService.Domain
{
    public class KeyPoint
    {
        public long Id { get; set; }
        public string Name { get;  set; }
        public double Longitude { get;  set; }
        public double Latitude { get;  set; }
        public string Description { get;  set; }
        public string Image { get;  set; }

        public long UserId { get;  set; }


        public long TourId { get;  set; }
        public KeyPoint() { }

        public KeyPoint(string name, double longitude, double latitude, string description, string image, long userId, long tourId)
        {
            Validate(name, longitude, latitude, description, image, userId);
            Name = name;
            Longitude = longitude;
            Latitude = latitude;
            Description = description;
            Image = image;
            UserId = userId;
            TourId = tourId;
        }

        public void Validate(string name, double longitude, double latitude, string description, string image, long userId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid Name.");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Invalid Description.");
            if (string.IsNullOrWhiteSpace(image)) throw new ArgumentException("Invalid Image.");
            if (longitude <= 0) throw new ArgumentException("Invalid longitude.");
            if (latitude <= 0) throw new ArgumentException("Invalid latitude.");
            
        }
    }
    public enum PublicStatus
    {
        PRIVATE = 0,
        REQUESTED_PUBLIC = 1,
        PUBLIC = 2
    }
}

