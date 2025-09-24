namespace ToursService.Integrations
{
    public class NatsOptions
    {
        public string Url { get; set; } = "nats://localhost:4222";
        public string ClientName { get; set; } = "tours-service";
    }
}
