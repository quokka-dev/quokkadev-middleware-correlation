using HttpContextMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class CorrelationMiddlewareUnitTest
    {
        private readonly string fakeId = "MY-FAKE-ID";
        private readonly string fakeIdFromRequest = "MY-FAKE-ID-FROM-REQUEST";

        public CorrelationMiddlewareUnitTest()
        {
        }

        [Fact(DisplayName = "Correct Response Header Should Be Set")]
        public async Task CorrectResponseHeaderShouldBeSet()
        {
            // Arrange
            CorrelationMiddleware middleware = GetCorrelationMiddleware(new CorrelationOptions()
            {
                DefaultHeaderName = "My-X-CorrelationId",
                EnrichLog = true,
                LogPropertyName = "CorrelationId",
                TryToUseRequestHeader = true,
                ValidRequestHeaders = new string[] { "X-CorrelationId" },
                WriteCorrelationIDToResponse = true
            });

            ICorrelationService service = GetCorrelationService();

            var context = new HttpContextMock();
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            context.ResponseMock.Mock.Setup(r => r.Headers).Returns(headerDictionary);

            ICorrelationIdProvider idProvider = GetCorrelationIdProvider();

            // Act
            await middleware.InvokeAsync(context, service, idProvider);

            // Assert            
            context.ResponseMock.HeadersMock.Mock.Verify(d => d.Add("My-X-CorrelationId", fakeId), Times.Once);
        }

        private CorrelationMiddleware GetCorrelationMiddleware(CorrelationOptions options, ILogger<CorrelationMiddleware>? logger = null)
        {
            RequestDelegate next = (HttpContext hc) => Task.CompletedTask;

            CorrelationMiddleware middleware = new CorrelationMiddleware(next, options, logger ?? GetLogger());
            return middleware;
        }

        private ILogger<CorrelationMiddleware> GetLogger()
        {
            var mock = new Mock<ILogger<CorrelationMiddleware>>();

            return mock.Object;
        }

        private ICorrelationService GetCorrelationService()
        {

            var mock = new Mock<ICorrelationService>();
            mock.Setup(m => m.GetCurrentCorrelationID()).Returns(fakeId);
            return mock.Object;
        }

        private ICorrelationIdProvider GetCorrelationIdProvider()
        {
            var mock = new Mock<ICorrelationIdProvider>();
            mock.Setup(m => m.GenerateCorrelationId()).Returns(fakeId);
            return mock.Object;
        }
    }
}
