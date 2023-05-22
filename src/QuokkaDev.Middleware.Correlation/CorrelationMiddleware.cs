using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QuokkaDev.Middleware.Correlation
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly CorrelationOptions options;
        private readonly ILogger<CorrelationMiddleware> logger;

        public CorrelationMiddleware(RequestDelegate next, CorrelationOptions options, ILogger<CorrelationMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext, ICorrelationService correlationService, ICorrelationIdProvider correlationIdProvider)
        {
            if (httpContext == null)
            {
                throw new ArgumentException("", nameof(httpContext));
            }

            var headerName = GetHeaderName(httpContext);
            var correlationID = GetCorrelationId(httpContext, headerName, correlationIdProvider);
            correlationService?.SetCorrelationID(correlationID);

            if (options.WriteCorrelationIDToResponse)
            {
                httpContext.Response.Headers.Add(headerName ?? options.DefaultHeaderName, correlationID);
            }

            if (options.EnrichLog)
            {
                using (logger.BeginScope<Dictionary<string, object>>(GetLogEnrichmentData(correlationID)))
                {
                    await next(httpContext).ConfigureAwait(false);
                }
            }
            else
            {
                await next(httpContext).ConfigureAwait(false);
            }
        }

        private Dictionary<string, object> GetLogEnrichmentData(string correlationID)
        {
            var loggerState = new Dictionary<string, object>
            {
                { options.LogPropertyName, correlationID }
            };
            return loggerState;
        }

        private static string GetCorrelationId(HttpContext httpContext, string? headerName, ICorrelationIdProvider correlationIdProvider)
        {
            string? correlationId = null;

            if (!string.IsNullOrEmpty(headerName))
            {
                correlationId = httpContext.Request.Headers[headerName];
            }

            return string.IsNullOrWhiteSpace(correlationId) ? correlationIdProvider.GenerateCorrelationId() : correlationId;
        }

        private static string? GetHeaderName(HttpContext httpContext)
        {
            string? headerName = null;
            var headers = httpContext.Request.Headers;
            if (options.TryToUseRequestHeader && options.ValidRequestHeaders != null)
            {
                headerName = options.ValidRequestHeaders.FirstOrDefault(validRequestHeader => headers.ContainsKey(validRequestHeader));
            }
            return headerName;
        }
    }
}
