using System.Text.Json;

namespace ToursService.Integrations.Saga
{
    public static class SagaSubjects
    {
        // --- Payments ---
        public const string PaymentsValidate = "payments.validate";
        public const string PaymentsLock = "payments.lock";
        public const string PaymentsFinalize = "payments.finalize";
        public const string PaymentsCompensate = "payments.compensate";

        // --- Tours ---
        public const string ToursExecCreate = "tours.exec.create";
        public const string ToursExecActivate = "tours.exec.activate";
        public const string ToursExecCompensate = "tours.exec.compensate";
    }

    public static class SagaJson
    {
        public static readonly JsonSerializerOptions Opts = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    // COMMAND: orkestrator (Tours) → Payments
    public record ValidatePurchaseCommand(
        long UserId,
        long TourId,
        long ExecutionId,
        string CorrelationId
    );

    public enum ValidatePurchaseStatus { Ok, NotFound, Forbidden, Error }

    // REPLY: Payments → orkestrator (Tours)
    public record ValidatePurchaseReply(
        ValidatePurchaseStatus Status,
        string? Message,
        string? Reason,
        string CorrelationId
    );

    // Lock
    public sealed record PaymentLockCommand(long UserId, long TourId, long ExecutionId, string CorrelationId);
    public sealed record PaymentLockReply(bool Success, string? Reason, string CorrelationId);

    // Finalize
    public sealed record PaymentFinalizeCommand(long ExecutionId, string CorrelationId);
    public sealed record PaymentFinalizeReply(bool Success, string? Reason, string CorrelationId);

    // Compensate
    public sealed record PaymentCompensateCommand(long ExecutionId, string? Reason, string CorrelationId);
    public sealed record PaymentCompensateReply(bool Success, string? Reason, string CorrelationId);
}
