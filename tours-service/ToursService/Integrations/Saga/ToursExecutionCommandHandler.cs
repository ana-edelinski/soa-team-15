using ToursService.UseCases;

namespace ToursService.Integrations.Saga
{
    /// <summary>
    /// Tours “participant” koji sluša orkestratorove komande preko NATS-a:
    /// - tours.exec.create      → napravi PENDING TourExecution i vrati executionId
    /// - tours.exec.activate    → prebaci PENDING u ACTIVE
    /// - tours.exec.compensate  → kompenzacija (npr. abandon/delete) ako korak padne
    /// </summary>
    public class ToursExecutionCommandHandler : IHostedService, IDisposable
    {
        private readonly INatsSagaBus _bus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ToursExecutionCommandHandler> _log;

        private IDisposable? _subCreate;
        private IDisposable? _subActivate;
        private IDisposable? _subCompensate;

        public ToursExecutionCommandHandler(
            INatsSagaBus bus,
            IServiceScopeFactory scopeFactory,
            ILogger<ToursExecutionCommandHandler> log)
        {
            _bus = bus;
            _scopeFactory = scopeFactory;
            _log = log;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subCreate = _bus.Respond<ExecCreateCommand, ExecCreateReply>(
                SagaSubjects.ToursExecCreate,
                (cmd, ct) => WithService(exec => HandleCreateAsync(exec, cmd, ct)));

            _subActivate = _bus.Respond<ExecActivateCommand, ExecActivateReply>(
                SagaSubjects.ToursExecActivate,
                (cmd, ct) => WithService(exec => HandleActivateAsync(exec, cmd, ct)));

            _subCompensate = _bus.Respond<ExecCompensateCommand, ExecCompensateReply>(
                SagaSubjects.ToursExecCompensate,
                (cmd, ct) => WithService(exec => HandleCompensateAsync(exec, cmd, ct)));

            _log.LogInformation("ToursExecutionCommandHandler subscribed on {Create}, {Activate}, {Compensate}",
                SagaSubjects.ToursExecCreate, SagaSubjects.ToursExecActivate, SagaSubjects.ToursExecCompensate);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { _subCreate?.Dispose(); } catch { }
            try { _subActivate?.Dispose(); } catch { }
            try { _subCompensate?.Dispose(); } catch { }
            _subCreate = _subActivate = _subCompensate = null;
        }

        // === Helper za kreiranje scope-a ===
        private async Task<TReply> WithService<TReply>(Func<ITourExecutionService, Task<TReply>> action)
        {
            using var scope = _scopeFactory.CreateScope();
            var exec = scope.ServiceProvider.GetRequiredService<ITourExecutionService>();
            return await action(exec);
        }

        // === Handleri (koriste ITourExecutionService iz scope-a) ===

        private async Task<ExecCreateReply> HandleCreateAsync(ITourExecutionService exec, ExecCreateCommand cmd, CancellationToken ct)
        {
            try
            {
                var execId = await exec.CreatePendingAsync(
                    touristId: cmd.UserId,
                    tourId: cmd.TourId,
                    locationId: cmd.LocationId,
                    correlationId: cmd.CorrelationId,
                    ct: ct);

                return new ExecCreateReply(true, execId, null, cmd.CorrelationId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Create execution failed for TourId={TourId}, UserId={UserId}", cmd.TourId, cmd.UserId);
                return new ExecCreateReply(false, null, ex.Message, cmd.CorrelationId);
            }
        }

        private async Task<ExecActivateReply> HandleActivateAsync(ITourExecutionService exec, ExecActivateCommand cmd, CancellationToken ct)
        {
            try
            {
                await exec.ActivateAsync(cmd.ExecutionId, ct);
                return new ExecActivateReply(true, null, cmd.CorrelationId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Activate execution failed for ExecutionId={ExecutionId}", cmd.ExecutionId);
                return new ExecActivateReply(false, ex.Message, cmd.CorrelationId);
            }
        }

        private async Task<ExecCompensateReply> HandleCompensateAsync(ITourExecutionService exec, ExecCompensateCommand cmd, CancellationToken ct)
        {
            try
            {
                await exec.CompensateAsync(cmd.ExecutionId, cmd.Reason ?? "Compensated by orchestrator", ct);
                return new ExecCompensateReply(true, null, cmd.CorrelationId);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Compensate execution failed for ExecutionId={ExecutionId}", cmd.ExecutionId);
                return new ExecCompensateReply(false, ex.Message, cmd.CorrelationId);
            }
        }

        // === Poruke za Tours “participant” ===

        public sealed record ExecCreateCommand(long UserId, long TourId, long LocationId, string CorrelationId);
        public sealed record ExecCreateReply(bool Success, long? ExecutionId, string? Reason, string CorrelationId);

        public sealed record ExecActivateCommand(long ExecutionId, string CorrelationId);
        public sealed record ExecActivateReply(bool Success, string? Reason, string CorrelationId);

        public sealed record ExecCompensateCommand(long ExecutionId, string? Reason, string CorrelationId);
        public sealed record ExecCompensateReply(bool Success, string? Reason, string CorrelationId);
    }
}
