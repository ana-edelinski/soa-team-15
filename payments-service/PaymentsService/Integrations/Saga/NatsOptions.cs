namespace PaymentsService.Integrations.Saga
{
    public class NatsOptions
    {
        public string Url { get; set; } = "nats://localhost:4222";
        public string ClientName { get; set; } = "payments-service";
    }
}
