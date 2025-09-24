namespace ToursService.Integrations.Saga
{

    public interface IPaymentSagaClient
    {
        Task<ValidatePurchaseReply> ValidateBeforeStartAsync(long userId, long tourId, long executionId, CancellationToken ct = default);
    }
    public sealed class PaymentSagaClient: IPaymentSagaClient
    {
        private readonly INatsSagaBus _bus;

        public PaymentSagaClient(INatsSagaBus bus) => _bus = bus;

        public async Task<ValidatePurchaseReply> ValidateBeforeStartAsync(long userId, long tourId, long executionId, CancellationToken ct = default)
        {
            var corr = Guid.NewGuid().ToString("N");
            var cmd = new ValidatePurchaseCommand(userId, tourId, executionId, corr);

            try
            {
                var reply = await _bus.RequestAsync<ValidatePurchaseCommand, ValidatePurchaseReply>(
                    SagaSubjects.PaymentsValidate, cmd, TimeSpan.FromSeconds(3), ct);

                return reply ?? new ValidatePurchaseReply(
                    ValidatePurchaseStatus.Error,
                    "No reply from payments",
                    null,
                    corr
                );
            }
            catch (NATS.Client.NATSNoRespondersException)
            {
                // Niko ne sluša → tretiraj kao error
                return new ValidatePurchaseReply(
                    ValidatePurchaseStatus.Error,
                    "Payments service unavailable",
                    "No responders",
                    corr
                );
            }
            catch (Exception ex)
            {
                return new ValidatePurchaseReply(
                    ValidatePurchaseStatus.Error,
                    "Payments validation failed",
                    ex.Message,
                    corr
                );
            }

        }
    }
}
