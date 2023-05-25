using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QuokkaDev.Middleware.Correlation
{
    public class CorrelatingHttpMessageHandler : DelegatingHandler
    {
        private readonly CorrelationOptions options;
        private readonly IHttpContextAccessor accessor;
        private ICorrelationForwarder? forwarder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler"/> class using specified context accessor.
        /// </summary>
        /// <param name="correlationContextAccessor">The correlation context accessor.</param>
        /// <param name="options">The client correlation options.</param>
        public CorrelatingHttpMessageHandler(IOptions<CorrelationOptions> options, IHttpContextAccessor accessor, ICorrelationForwarder? forwarder = null)
            : base()
        {
            this.options = options.Value;
            this.accessor = accessor;
            this.forwarder = forwarder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler"/> class using specified context accessor.
        /// </summary>
        /// <param name="correlationContextAccessor">The correlation context accessor.</param>
        /// <param name="options">The client correlation options.</param>
        /// <param name="innerHandler">The inner handler.</param>
        public CorrelatingHttpMessageHandler(IOptions<CorrelationOptions> options, HttpMessageHandler innerHandler, IHttpContextAccessor accessor, ICorrelationForwarder? forwarder = null)
            : base(innerHandler)
        {
            this.options = options.Value;
            this.accessor = accessor;
            this.forwarder = forwarder;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return SendInternalAsync(request, cancellationToken);
        }

        private Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationService = accessor.HttpContext.RequestServices.GetRequiredService<ICorrelationService>();

            var correlationId = correlationService.GetCurrentCorrelationID();
            if (correlationId != null && !request.Headers.Contains(options.DefaultHeaderName))
            {
                forwarder ??= new HeaderCorrelationForwarder(request, options.DefaultHeaderName);
                forwarder.Forward(correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private sealed class HeaderCorrelationForwarder : ICorrelationForwarder
        {
            private readonly HttpRequestMessage request;
            private readonly string headerName;

            public HeaderCorrelationForwarder(HttpRequestMessage request, string headerName)
            {
                this.request = request;
                this.headerName = headerName;
            }

            public void Forward(string correlationId)
            {
                request.Headers.TryAddWithoutValidation(headerName, correlationId);
            }
        }
    }
}
