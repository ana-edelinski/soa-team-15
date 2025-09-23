using NATS.Client;
using System.Text;
using System.Text.Json;

namespace ToursService.Integrations.Saga
{
    public interface INatsSagaBus
    {
        Task<TReply?> RequestAsync<TReq, TReply>(
            string subject,
            TReq payload,
            TimeSpan? timeout = null,
            CancellationToken ct = default);

        IDisposable Respond<TReq, TRes>(
            string subject,
            Func<TReq, CancellationToken, Task<TRes>> handler);
    }

    public class NatsSagaBus : INatsSagaBus
    {
        private readonly NatsConnectionProvider _connProvider;

        public NatsSagaBus(NatsConnectionProvider connProvider)
            => _connProvider = connProvider;

        public async Task<TReply?> RequestAsync<TReq, TReply>(
            string subject,
            TReq payload,
            TimeSpan? timeout = null,
            CancellationToken ct = default)
        {
            var conn = _connProvider.Connection; // IConnection
            var json = JsonSerializer.Serialize(payload, SagaJson.Opts);
            var data = Encoding.UTF8.GetBytes(json);
            var to = timeout ?? TimeSpan.FromSeconds(3);

            // Klasičan NATS request (bez headers overload-a)
            var reply = await conn.RequestAsync(subject, data, (int)to.TotalMilliseconds);

            if (reply?.Data == null || reply.Data.Length == 0) return default;
            return JsonSerializer.Deserialize<TReply>(reply.Data, SagaJson.Opts);
        }

        public IDisposable Respond<TReq, TRes>(
            string subject,
            Func<TReq, CancellationToken, Task<TRes>> handler)
        {
            var conn = _connProvider.Connection; // IConnection
            var sub = conn.SubscribeAsync(subject); // IAsyncSubscription

            sub.MessageHandler += async (_, args) =>
            {
                try
                {
                    var req = JsonSerializer.Deserialize<TReq>(args.Message.Data, SagaJson.Opts)!;
                    var res = await handler(req, CancellationToken.None);
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(res, SagaJson.Opts);

                    if (!string.IsNullOrEmpty(args.Message.Reply))
                        conn.Publish(args.Message.Reply, bytes);
                }
                catch (Exception ex)
                {
                    var err = JsonSerializer.SerializeToUtf8Bytes(new { error = ex.Message }, SagaJson.Opts);
                    if (!string.IsNullOrEmpty(args.Message.Reply))
                        conn.Publish(args.Message.Reply, err);
                }
            };

            sub.Start();               // pokreni async subscription
            return sub;                // IAsyncSubscription je IDisposable
        }
    }
}
