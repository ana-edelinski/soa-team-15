using ToursService.Domain;

namespace ToursService.Dtos
{
    public class TourTransportTimeDto
{
    public long Id { get; set; }
    public long TourId { get; set; }
    public TransportType Type { get; set; }
    public int Minutes { get; set; }
}

    public class TransportTimeRequest
    {
        public TransportType Type { get; set; }
        public int Minutes { get; set; }
    }
    
}
