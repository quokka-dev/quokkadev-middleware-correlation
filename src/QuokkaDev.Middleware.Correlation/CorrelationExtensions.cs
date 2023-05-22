using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace QuokkaDev.Middleware.Correlation
{
    public static class CorrelationExtensions
    {
        public static IApplicationBuilder UseCorrelation(this IApplicationBuilder builder, Action<CorrelationOptions>? configureOptions = null)
        {
            configureOptions ??= ((CorrelationOptions _) => { });
            var options = new CorrelationOptions
            {
                TryToUseRequestHeader = true,
                ValidRequestHeaders = new string[] { Constants.DEFAULT_CORRELATION_HEADER_NAME },
                EnrichLog = true,
                LogPropertyName = Constants.DEFAULT_CORRELATION_LOG_PROPERTY,
                WriteCorrelationIDToResponse = true,
                DefaultHeaderName = Constants.DEFAULT_CORRELATION_HEADER_NAME
            };

            configureOptions?.Invoke(options);

            return builder.UseMiddleware<CorrelationMiddleware>(options);
        }

        public static IServiceCollection AddCorrelation(this IServiceCollection services)
        {
            services.TryAddScoped<ICorrelationService, CorrelationService>();
            services.TryAddTransient<ICorrelationIdProvider, GuidCorrelationIdProvider>();
            return services;
        }

        /// <summary>
        /// Adds services required for adding correlation id to each outgoing <see cref="HttpClient"/> request.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the services to.</param>
        /// <param name="requestHeader">The request header name to set the correlation id in.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> so that additional calls can be chained.</returns>
        public static IHttpClientBuilder CorrelateRequests(this IHttpClientBuilder builder, string requestHeader = Constants.DEFAULT_CORRELATION_HEADER_NAME)
        {
            return builder.CorrelateRequests(options => options.DefaultHeaderName = requestHeader);
        }

        /// <summary>
        /// Adds services required for adding correlation id to each outgoing <see cref="HttpClient"/> request.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the services to.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to configure <see cref="CorrelateClientOptions"/>.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> so that additional calls can be chained.</returns>
        public static IHttpClientBuilder CorrelateRequests(this IHttpClientBuilder builder, IConfiguration configuration)
        {
            return builder.CorrelateRequests(configuration.Bind);
        }

        /// <summary>
        /// Adds services required for adding correlation id to each outgoing <see cref="HttpClient"/> request.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure <see cref="CorrelateClientOptions"/>.</param>
        /// <returns>The <see cref="IHttpClientBuilder"/> so that additional calls can be chained.</returns>
        public static IHttpClientBuilder CorrelateRequests(this IHttpClientBuilder builder, Action<CorrelationOptions> configureOptions)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            builder.Services.AddCorrelation();

            builder.Services.AddTransient<CorrelatingHttpMessageHandler>();
            builder.Services.Configure(builder.Name, configureOptions);
            builder.AddHttpMessageHandler(s =>
            {
                var allClientOptions = s.GetRequiredService<IOptionsSnapshot<CorrelationOptions>>();
                var thisClientOptions = new OptionsWrapper<CorrelationOptions>(allClientOptions.Get(builder.Name));
                return ActivatorUtilities.CreateInstance<CorrelatingHttpMessageHandler>(
                    s,
                    (IOptions<CorrelationOptions>)thisClientOptions
                );
            });

            return builder;
        }
    }
}
