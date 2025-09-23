using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Collections.Generic;
using Serilog.Sinks.Grafana.Loki;

namespace Shared.Logging;

public static class SerilogSetup
{
    public static IHostBuilder UseDefaultSerilog(this IHostBuilder host)
    {
        host.UseSerilog((ctx, cfg) =>
        {
            var svc = ctx.Configuration["ServiceName"] ?? AppDomain.CurrentDomain.FriendlyName;
            var env = ctx.HostingEnvironment.EnvironmentName;

            cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
               .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
               .Enrich.FromLogContext()
               .Enrich.WithProperty("service", svc)
               .Enrich.WithProperty("env", env)
               .WriteTo.Console(new CompactJsonFormatter());

    
            var lokiUrl = ctx.Configuration["Logging:Loki:Url"];
            if (!string.IsNullOrWhiteSpace(lokiUrl))
            {
                cfg.WriteTo.GrafanaLoki(
                    lokiUrl,
                    labels: new List<LokiLabel>
                    {
                        new LokiLabel { Key = "service", Value = svc },
                        new LokiLabel { Key = "env", Value = env }
                    }
                );
            }
        });
        return host;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (ctx, next) =>
        {
            const string header = "X-Correlation-ID";
            if (!ctx.Request.Headers.TryGetValue(header, out var cid) || string.IsNullOrWhiteSpace(cid))
                cid = Guid.NewGuid().ToString("n");

            using (LogContext.PushProperty("correlationId", cid.ToString()))
            {
                ctx.Response.Headers[header] = cid!;
                await next();
            }
        });
    }
}
