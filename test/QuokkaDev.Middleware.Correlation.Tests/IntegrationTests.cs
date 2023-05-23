using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class IntegrationTests
    {


        public IntegrationTests()
        {
        }

        [Fact(DisplayName = "Correlation Id From Request Should Be Returned")]
        public async Task CorrelationIdFromRequestShouldBeReturned()
        {
            var hostBuilder = GetWebHostBuilder();

            var testHost = await hostBuilder.StartAsync();
            var client = testHost.GetTestClient();

            client.DefaultRequestHeaders.Add("X-Correlation-Id", "MyCorrelationId");

            // Act
            var response = await client.GetAsync("/test");

            // Assert            
            response.Headers.GetValues("X-Correlation-Id").Should().Contain("MyCorrelationId");
        }

        [Fact(DisplayName = "Correlation Id Should Be Generated")]
        public async Task CorrelationIdShouldBeGenerated()
        {
            var hostBuilder = GetWebHostBuilder();

            var testHost = await hostBuilder.StartAsync();
            var client = testHost.GetTestClient();

            // Act
            var response = await client.GetAsync("/test");
            var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();

            // Assert            
            correlationId.Should().NotBeNull();
            correlationId.Should().NotBeEmpty();
            correlationId.Should().MatchRegex(@"^[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}$");
        }

        [Fact(DisplayName = "Correlation Id Should Be Forwarded")]
        public async Task CorrelationIdShouldBeForwarded()
        {
            var hostBuilder = GetWebHostBuilder(forwardCorrelationId: true);

            var testHost = await hostBuilder.StartAsync();
            var client = testHost.GetTestClient();

            // Act
            var response = await client.GetAsync("/test?forward=true");
            var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();

            var headerPresent = response.Headers.GetValues("X-Header-Present").FirstOrDefault();
            var headerValue = response.Headers.GetValues("X-Header-Value").FirstOrDefault();

            // Assert 
            headerPresent.Should().NotBeNull();
            headerPresent.Should().NotBeEmpty();
            headerPresent.Should().Be("True");

            headerValue.Should().NotBeNull();
            headerValue.Should().NotBeEmpty();
            headerValue.Should().MatchRegex(@"^[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}$");
            headerValue.Should().BeEquivalentTo(correlationId);
        }

        private IHostBuilder GetWebHostBuilder(Action<CorrelationOptions>? configureOptions = null, bool forwardCorrelationId = false)
        {
            var hostBuilder = new HostBuilder().ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.ConfigureServices(services =>
                {
                    services.AddHttpContextAccessor();
                    services.AddCorrelation();

                    var builder = services.AddHttpClient("MyTestClient");
                    if (forwardCorrelationId)
                    {
                        builder.ForwardCorrelationId().AddHttpMessageHandler<CheckHeaderHandler>();
                        services.AddTransient<CheckHeaderHandler>();
                    }
                });

                webHostBuilder.Configure(applicationBuilder =>
                {
                    applicationBuilder.UseCorrelation(configureOptions);
                    applicationBuilder.Run(async context =>
                    {
                        if (context.Request.Query.Any(p => p.Key == "forward" && p.Value == "true"))
                        {
                            var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                            var client = factory.CreateClient("MyTestClient");

                            await client.GetAsync("https://localhost:55555/fake");
                        }

                        context.Response.StatusCode = 200;
                    });
                });
                webHostBuilder.UseTestServer();
            });

            return hostBuilder;
        }
    }

    public class CheckHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CheckHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool headerPresent = request.Headers.Any(h => h.Key == "X-Correlation-Id");
            httpContextAccessor.HttpContext?.Response.Headers.Add("X-Header-Present", headerPresent.ToString());

            if (headerPresent)
            {
                string headerValue = request.Headers.First(h => h.Key == "X-Correlation-Id").Value.First();
                httpContextAccessor.HttpContext?.Response.Headers.Add("X-Header-Value", headerValue);
            }

            return Task.FromResult(new Mock<HttpResponseMessage>().Object);
        }
    }
}
