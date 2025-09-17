using ToursService.Domain;

namespace ToursService.Dtos
{
    public class KeyPointDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public long? UserId { get; set; }

        public long? TourId { get; set; }
        public IFormFile PictureFile { get; set;}
        public PublicStatus PublicStatus { get; set; }
        public KeyPointDto() { }


        public KeyPointDto(KeyPoint kp)
        {
            Id = kp.Id;
            Name = kp.Name;
            Description = kp.Description;
            Image = kp.Image;
            Latitude = kp.Latitude;
            Longitude = kp.Longitude;
            TourId = kp.TourId;
        }
    }

    public enum PublicStatus
    {
        PRIVATE = 0,
        REQUESTED_PUBLIC = 1,
        PUBLIC = 2
    }
}
