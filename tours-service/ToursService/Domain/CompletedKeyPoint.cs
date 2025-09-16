using System.Text.Json.Serialization;
using ToursService.Utils;

namespace ToursService.Domain
{
    public class CompletedKeyPoint: ValueObject
    {
        public long KeyPointId { get; init; }

        public DateTime CompletedTime { get; init; }

        [JsonConstructor]
        public CompletedKeyPoint(long keyPointId, DateTime completedTime)
        {
            KeyPointId = keyPointId;
            CompletedTime = completedTime;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return KeyPointId;
            yield return CompletedTime;
        }
    }
}
