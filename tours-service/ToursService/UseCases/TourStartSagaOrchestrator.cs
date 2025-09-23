using ToursService.Integrations.Saga;

namespace ToursService.UseCases
{
    public class TourStartSagaOrchestrator
    {
        private readonly INatsSagaBus _bus;
        private readonly IPaymentSagaClient _payments;
        private readonly ILogger<TourStartSagaOrchestrator> _log;

        public TourStartSagaOrchestrator(
            INatsSagaBus bus,
            IPaymentSagaClient payments,
            ILogger<TourStartSagaOrchestrator> log)
        {
            _bus = bus;
            _payments = payments;
            _log = log;
        }

        public async Task<StartTourResult> StartTourSagaAsync(
        long userId,
        long tourId,
        long locationId,
        CancellationToken ct)
            {
                var corr = Guid.NewGuid().ToString("N"); // correlation id za praćenje

                try
                {
                    ct.ThrowIfCancellationRequested();

                    // === 1. Kreiraj pending execution u Tours ===
                    var createReply = await _bus.RequestAsync<
                        ToursExecutionCommandHandler.ExecCreateCommand,
                        ToursExecutionCommandHandler.ExecCreateReply>(
                            SagaSubjects.ToursExecCreate,
                            new ToursExecutionCommandHandler.ExecCreateCommand(userId, tourId, locationId, corr),
                            timeout: TimeSpan.FromSeconds(3),
                            ct: ct);

                    if (createReply == null || !createReply.Success || createReply.ExecutionId == null)
                    {
                        return new StartTourResult(false, "Failed to create tour execution");
                    }

                    var executionId = createReply.ExecutionId.Value;

                    // === 2. Validacija uplata u Payments ===
                    var validate = await _payments.ValidateBeforeStartAsync(userId, tourId, executionId, ct);

                    if (validate.Status != ValidatePurchaseStatus.Ok)
                    {
                        await CompensateToursAsync(executionId, validate.Reason, corr, ct);
                        return new StartTourResult(false, $"Payment validation failed: {validate.Reason}");
                    }

                    // === 3. Zaključavanje tokena u Payments ===
                    var lockReply = await _bus.RequestAsync<
                        PaymentLockCommand,
                        PaymentLockReply>(
                            SagaSubjects.PaymentsLock,
                            new PaymentLockCommand(userId, tourId, executionId, corr),
                            timeout: TimeSpan.FromSeconds(3),
                            ct: ct);

                    if (lockReply == null || !lockReply.Success)
                    {
                    // Kompenzacija: otkaži execution u Tours
                    await _bus.RequestAsync<
                        ToursExecutionCommandHandler.ExecCompensateCommand,
                        ToursExecutionCommandHandler.ExecCompensateReply>(
                            SagaSubjects.ToursExecCompensate,
                            new ToursExecutionCommandHandler.ExecCompensateCommand(executionId, "Lock failed", corr),
                            timeout: TimeSpan.FromSeconds(3),
                            ct: ct);

                    // 👇 fallback: otključaj uplatu u Payments (ako je stigla do pola)
                    await _bus.RequestAsync<
                        PaymentCompensateCommand,
                        PaymentCompensateReply>(
                            SagaSubjects.PaymentsCompensate,
                            new PaymentCompensateCommand(executionId, "Lock rollback", corr),
                            timeout: TimeSpan.FromSeconds(3),
                            ct: ct);

                    return new StartTourResult(false, $"Payment lock failed: {lockReply?.Reason}");
                }

                    // === 4. Aktivacija execution-a u Tours ===
                    var activate = await _bus.RequestAsync<
                        ToursExecutionCommandHandler.ExecActivateCommand,
                        ToursExecutionCommandHandler.ExecActivateReply>(
                            SagaSubjects.ToursExecActivate,
                            new ToursExecutionCommandHandler.ExecActivateCommand(executionId, corr),
                            timeout: TimeSpan.FromSeconds(3),
                            ct: ct);

                    if (activate == null || !activate.Success)
                    {
                        await CompensateToursAsync(executionId, "Failed to activate tour", corr, ct);
                        await CompensatePaymentsAsync(executionId, "Activation failed", corr, ct);
                        return new StartTourResult(false, "Failed to activate tour execution");
                    }

                    // === 5. Finalizacija u Payments ===
                    var finalizeReply = await _bus.RequestAsync<
                        PaymentFinalizeCommand,
                        PaymentFinalizeReply>(
                            SagaSubjects.PaymentsFinalize,
                            new PaymentFinalizeCommand(executionId, corr),
                            timeout: TimeSpan.FromSeconds(3),
                            ct: ct);

                    if (finalizeReply == null || !finalizeReply.Success)
                    {
                        // Kompenzacija u Tours i Payments
                        await CompensateToursAsync(executionId, finalizeReply?.Reason ?? "Failed to finalize payment", corr, ct);
                        await CompensatePaymentsAsync(executionId, "Finalize failed", corr, ct);
                        return new StartTourResult(false, $"Payment finalize failed: {finalizeReply?.Reason}");
                    }

                    return new StartTourResult(true, "Tour started successfully", executionId);
                }
                catch (OperationCanceledException)
                {
                    _log.LogWarning("StartTourSagaAsync cancelled for User {UserId}, Tour {TourId}", userId, tourId);
                    return new StartTourResult(false, "Request was cancelled");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Unexpected error in StartTourSagaAsync");
                    return new StartTourResult(false, $"Unexpected error: {ex.Message}");
                }
            }

        private async Task CompensateToursAsync(long executionId, string? reason, string corr, CancellationToken ct)
        {
            await _bus.RequestAsync<
                ToursExecutionCommandHandler.ExecCompensateCommand,
                ToursExecutionCommandHandler.ExecCompensateReply>(
                    SagaSubjects.ToursExecCompensate,
                    new ToursExecutionCommandHandler.ExecCompensateCommand(executionId, reason, corr),
                    timeout: TimeSpan.FromSeconds(3),
                    ct: ct);
        }

        private async Task CompensatePaymentsAsync(long executionId, string? reason, string corr, CancellationToken ct)
        {
            await _bus.RequestAsync<
                PaymentCompensateCommand,
                PaymentCompensateReply>(
                    SagaSubjects.PaymentsCompensate,
                    new PaymentCompensateCommand(executionId, reason, corr),
                    timeout: TimeSpan.FromSeconds(3),
                    ct: ct);
        }

    }

    public record StartTourResult(bool Success, string Message, long? ExecutionId = null);
}

