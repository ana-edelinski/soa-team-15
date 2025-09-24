using PaymentsService.UseCases;

namespace PaymentsService.Integrations.Saga
{
    /// <summary>
    /// Payments “participant” koji sluša orkestratorove komande preko NATS-a:
    /// - payments.validate   → proveri da li korisnik ima uplatu za turu
    /// - payments.lock       → rezerviši uplatu dok traje saga
    /// - payments.finalize   → označi uplatu kao iskorišćenu (Consumed)
    /// - payments.compensate → otključa uplatu ako saga pukne
    /// </summary>
    public class PaymentsCommandHandler : IHostedService, IDisposable
    {
        private readonly INatsSagaBus _bus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentsCommandHandler> _log;

        private IDisposable? _subValidate;
        private IDisposable? _subLock;
        private IDisposable? _subFinalize;
        private IDisposable? _subCompensate;

        public PaymentsCommandHandler(
            INatsSagaBus bus,
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentsCommandHandler> log)
        {
            _bus = bus;
            _scopeFactory = scopeFactory;
            _log = log;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subValidate = _bus.Respond<ValidatePurchaseCommand, ValidatePurchaseReply>(
                SagaSubjects.PaymentsValidate,
                (cmd, ct) => WithService(svc => HandleValidateAsync(svc, cmd, ct)));

            _subLock = _bus.Respond<PaymentLockCommand, PaymentLockReply>(
                SagaSubjects.PaymentsLock,
                (cmd, ct) => WithService(svc => HandleLockAsync(svc, cmd, ct)));

            _subFinalize = _bus.Respond<PaymentFinalizeCommand, PaymentFinalizeReply>(
                SagaSubjects.PaymentsFinalize,
                (cmd, ct) => WithService(svc => HandleFinalizeAsync(svc, cmd, ct)));

            _subCompensate = _bus.Respond<PaymentCompensateCommand, PaymentCompensateReply>(
                SagaSubjects.PaymentsCompensate,
                (cmd, ct) => WithService(svc => HandleCompensateAsync(svc, cmd, ct)));

            _log.LogInformation("PaymentsCommandHandler subscribed on {Validate}, {Lock}, {Finalize}, {Compensate}",
                SagaSubjects.PaymentsValidate, SagaSubjects.PaymentsLock, SagaSubjects.PaymentsFinalize, SagaSubjects.PaymentsCompensate);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { _subValidate?.Dispose(); } catch { }
            try { _subLock?.Dispose(); } catch { }
            try { _subFinalize?.Dispose(); } catch { }
            try { _subCompensate?.Dispose(); } catch { }
            _subValidate = _subLock = _subFinalize = _subCompensate = null;
        }

        // === Helper za scope ===
        private async Task<TReply> WithService<TReply>(Func<IPurchaseService, Task<TReply>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            return await action(svc);
        }

        // === Handleri (koriste IPaymentService) ===

        private async Task<ValidatePurchaseReply> HandleValidateAsync(IPurchaseService svc, ValidatePurchaseCommand cmd, CancellationToken ct)
        {
            // 👉 DODATI u servis: ValidatePurchase(userId, tourId, executionId)
            return await svc.ValidatePurchase(cmd.UserId, cmd.TourId, cmd.ExecutionId, cmd.CorrelationId, ct);
        }

        private async Task<PaymentLockReply> HandleLockAsync(IPurchaseService svc, PaymentLockCommand cmd, CancellationToken ct)
        {
            // 👉 DODATI u servis: LockPurchase(userId, tourId, executionId, correlationId)
            return await svc.LockPurchase(cmd.UserId, cmd.TourId, cmd.ExecutionId, cmd.CorrelationId, ct);
        }

        private async Task<PaymentFinalizeReply> HandleFinalizeAsync(IPurchaseService svc, PaymentFinalizeCommand cmd, CancellationToken ct)
        {
            // 👉 DODATI u servis: FinalizePurchase(executionId, correlationId)
            return await svc.FinalizePurchase(cmd.ExecutionId, cmd.CorrelationId, ct);
        }

        private async Task<PaymentCompensateReply> HandleCompensateAsync(IPurchaseService svc, PaymentCompensateCommand cmd, CancellationToken ct)
        {
            // 👉 DODATI u servis: CompensatePurchase(executionId, reason, correlationId)
            return await svc.CompensatePurchase(cmd.ExecutionId, cmd.Reason ?? "Compensated by orchestrator", cmd.CorrelationId, ct);
        }
    }
}
