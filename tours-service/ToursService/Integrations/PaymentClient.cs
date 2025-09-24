using System.Net.Http.Headers;
using System.Text.Json;

namespace ToursService.Integrations
{
    public class PaymentClient : IPaymentClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private readonly ILogger<PaymentClient> _log;

        public PaymentClient(HttpClient http, IHttpContextAccessor ctx, ILogger<PaymentClient> log)
        {
            _http = http;
            _ctx = ctx;
            _log = log;
        }

        public async Task<List<long>> GetPurchasedIdsAsync(long userId, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/shopping/purchases/{userId}");

            // Prosledi Bearer token prema Payments servisu
            var auth = _ctx.HttpContext?.Request?.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(auth))
                req.Headers.TryAddWithoutValidation("Authorization", auth);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("Payments returned {Status} for user {UserId}", res.StatusCode, userId);
                return new List<long>();
            }

            // Pokušaj: raw lista [1,2,3]
            var text = await res.Content.ReadAsStringAsync(ct);
            try
            {
                var ids = JsonSerializer.Deserialize<List<long>>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (ids != null) return ids;
            }
            catch (JsonException) { /* padamo na wrapper */ }

            // Fallback: wrapper { "tourIds": [1,2,3] }
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("tourIds", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array)
                {
                    return arr.EnumerateArray()
                              .Where(e => e.ValueKind == JsonValueKind.Number)
                              .Select(e => e.GetInt64())
                              .ToList();
                }
            }
            catch (JsonException) { }

            _log.LogWarning("Payments payload not understood: {Payload}", text);
            return new List<long>();
        }
    }
}
