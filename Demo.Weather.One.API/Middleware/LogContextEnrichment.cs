using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Weather.One.API.Middleware
{
    public class LogContextEnrichment
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";
        private readonly RequestDelegate next;

        public LogContextEnrichment(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        // public async Task Invoke(HttpContext context, UserContext userContext, HttpContext requestContext, IDiagnosticContext diagnosticContext)
        public async Task Invoke(HttpContext context, IDiagnosticContext diagnosticContext)
        {
            var correlationId = GetCorrelationId(context);

            /* 
             * Serilog supports two context-aware features that can be used to enhance your logs.
             * 
             * LOG CONTEXT - can be used to dynamically add and remove properties from the ambient "execution context",
             * for example, all messages written during a transaction might carry the id of that transaction, and so-on.
             * 
             * DIAGNOSTIC CONTEXT - is provides an execution context (similar to LogContext) with the advantage that it 
             * can be enriched throughout its lifetime. The request logging middleware then uses this to enrich the final 
             * "log completion event". This allows us to collapse many different log operations into a single log entry, 
             * containing information from many points in the request pipeline.
             * 
             * Source: https://benfoster.io/blog/serilog-best-practices/
             */

            diagnosticContext.Set("ApplicationName", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            diagnosticContext.Set("ApplicationVersion", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            diagnosticContext.Set("CorrelationId", correlationId);

            /* using (LogContext.PushProperty(CorrelationIdHeaderKey, correlationId)) {
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationIds))
                    {
                        context.Response.Headers.Add(CorrelationIdHeaderKey, correlationId);
                    }

                    return Task.CompletedTask;
                });

                await next(context);
            } */

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationIds))
                {
                    context.Response.Headers.Add(CorrelationIdHeaderKey, correlationId);
                }

                return Task.CompletedTask;
            });

            await next(context);
        }

        private string GetCorrelationId(HttpContext context)
        {
            context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationIds);
            var correlationId = correlationIds.FirstOrDefault() ?? context.TraceIdentifier; // 0HMI7TTLPCL5A:00000005
            return correlationId;
        }
    }
}
