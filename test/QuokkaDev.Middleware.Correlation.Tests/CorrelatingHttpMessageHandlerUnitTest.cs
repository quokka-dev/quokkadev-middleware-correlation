using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class CorrelatingHttpMessageHandlerUnitTest
    {
        public CorrelatingHttpMessageHandlerUnitTest()
        {
        }

        [Fact(DisplayName = "Supplied Options Should Be Used")]
        public async Task SuppliedOptionsShouldBeUsed()
        {
            // Arrange
            CorrelationOptions options = new() { DefaultHeaderName = "foo" };

            var mockOptions = new Mock<IOptions<CorrelationOptions>>();
            mockOptions.Setup(m => m.Value).Returns(options);

            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(m => m.HttpContext).Returns((HttpContext)null);

            HttpRequestMessage request = new();

            var handler = new CorrelatingHttpMessageHandler(mockOptions.Object, mockAccessor.Object);

            // Act
            await handler.;

            // Assert
            obj.Should().NotBeNull();
        }
    }
}
