using NATS.Client;
using System.Text;

namespace ToursService.Integrations
{
    public class NatsConnectionProvider: IDisposable
    {
        private readonly IConnection _conn;

        public NatsConnectionProvider(NatsOptions opt)
        {
            var cf = new ConnectionFactory();

            var o = ConnectionFactory.GetDefaultOptions();
            o.Url = opt.Url;
            o.Name = opt.ClientName;

            // malo robusnosti
            o.MaxReconnect = 60;                     // do 60 pokušaja
            o.ReconnectWait = 2000;                  // 2s između pokušaja
            o.Timeout = 5000;                        // 5s connect timeout
            o.AllowReconnect = true;

            _conn = cf.CreateConnection(o);
        }

        public IConnection Connection => _conn;

        /// <summary>
        /// Jednostavan helper za request/reply (komandni stil).
        /// </summary>
        public async Task<Msg> RequestAsync(string subject, byte[] payload, int timeoutMs = 5000)
        {
            // NATS.Client ima sync Request; ovde ga koristimo kroz Task.Run radi jednostavnosti
            return await Task.Run(() => _conn.Request(subject, payload, timeoutMs));
        }

        public async Task<Msg> RequestJsonAsync<T>(string subject, T payload, int timeoutMs = 5000)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            var reply = await RequestAsync(subject, bytes, timeoutMs);
            return reply;
        }

        public void Dispose()
        {
            try { _conn?.Drain(); } catch { /* ignore */ }
            try { _conn?.Dispose(); } catch { /* ignore */ }
        }
    }
}

