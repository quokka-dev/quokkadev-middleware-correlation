using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QuokkaDev.Middleware.Correlation
{
    public class CorrelatingHttpMessageHandler : DelegatingHandler
    {
        private readonly CorrelationOptions options;
        private readonly IHttpContextAccessor accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler"/> class using specified context accessor.
        /// </summary>
        /// <param name="correlationContextAccessor">The correlation context accessor.</param>
        /// <param name="options">The client correlation options.</param>
        public CorrelatingHttpMessageHandler(IOptions<CorrelationOptions> options, IHttpContextAccessor accessor)
            : base()
        {
            this.options = options.Value ?? new CorrelationOptions();
            this.accessor = accessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler"/> class using specified context accessor.
        /// </summary>
        /// <param name="correlationContextAccessor">The correlation context accessor.</param>
        /// <param name="options">The client correlation options.</param>
        /// <param name="innerHandler">The inner handler.</param>
        public CorrelatingHttpMessageHandler(IOptions<CorrelationOptions> options, HttpMessageHandler innerHandler, IHttpContextAccessor accessor)
            : base(innerHandler)
        {
            this.options = options.Value ?? new CorrelationOptions();
            this.accessor = accessor;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var correlationService = accessor.HttpContext.RequestServices.GetRequiredService<ICorrelationService>();

            var correlationId = correlationService.GetCurrentCorrelationID();
            if (correlationId != null && !request.Headers.Contains(options.DefaultHeaderName))
            {
                request.Headers.TryAddWithoutValidation(options.DefaultHeaderName, correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
