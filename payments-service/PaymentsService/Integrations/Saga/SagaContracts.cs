using System.Text.Json;

namespace PaymentsService.Integrations.Saga
{
    public static class SagaSubjects
    {
        // Request-Reply (command + async reply)
        public const string PaymentsValidate = "payments.validate";
        public const string PaymentsLock = "payments.lock";
        public const string PaymentsFinalize = "payments.finalize";
        public const string PaymentsCompensate = "payments.compensate";
    }

    public static class SagaJson
    {
        public static readonly JsonSerializerOptions Opts = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    // === Validate ===
    public record ValidatePurchaseCommand(
        long UserId,
        long TourId,
        long ExecutionId,
        string CorrelationId
    );

    public enum ValidatePurchaseStatus { Ok, NotFound, Forbidden, Error }

    public record ValidatePurchaseReply(
        ValidatePurchaseStatus Status,
        string? Message,
        string? Reason,
        string CorrelationId
    );

    // === Lock ===
    public record PaymentLockCommand(
        long UserId,
        long TourId,
        long ExecutionId,
        string CorrelationId
    );

    public record PaymentLockReply(
        bool Success,
        string? Message,
        string CorrelationId
    );

    // === Finalize ===
    public record PaymentFinalizeCommand(
        long ExecutionId,
        string CorrelationId
    );

    public record PaymentFinalizeReply(
        bool Success,
        string? Message,
        string CorrelationId
    );

    // === Compensate ===
    public record PaymentCompensateCommand(
        long ExecutionId,
        string? Reason,
        string CorrelationId
    );

    public record PaymentCompensateReply(
        bool Success,
        string? Message,
        string CorrelationId
    );
}
