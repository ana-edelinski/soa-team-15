using System.Text.Json.Serialization;

namespace ToursService.Dtos
{
    public class CompletedKeyPointDto
    {
        public long KeyPointId { get; set; }

        public DateTime CompletedTime { get; set; }

        [JsonConstructor]
        public CompletedKeyPointDto(long keyPointId, DateTime completedTime)
        {
            KeyPointId = keyPointId;
            CompletedTime = completedTime;
        }
    }
}
