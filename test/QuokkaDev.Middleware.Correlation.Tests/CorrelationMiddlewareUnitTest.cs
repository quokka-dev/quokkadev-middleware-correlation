using FluentAssertions;
using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class CorrelationMiddlewareUnitTest
    {
        private const string fakeId = "MY-FAKE-ID";
        private const string fakeIdFromRequest = "MY-FAKE-ID-FROM-REQUEST";
        private const string headerNameInRequest = "X-CorrelationId";
        private const string defaultHeaderName = "My-X-CorrelationId";

        public CorrelationMiddlewareUnitTest()
        {
        }

        [Fact(DisplayName = "Http Context Should Not Be NUll")]
        public async Task HttpContextShouldNotBeNull()
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
            ICorrelationIdProvider idProvider = GetCorrelationIdProvider();

            // Act

            var execution = async () => await middleware.InvokeAsync(null!, service, idProvider);

            // Assert           
            await execution.Should().ThrowAsync<ArgumentException>().WithParameterName("httpContext");
        }

        [Theory(DisplayName = "Correct Header And Id Should Be Used")]
        [MemberData(nameof(GetValues))]
        public async Task CorrectHeaderAndIdShouldBeUsed(bool writeCorrelationIDToResponse, bool tryToUseRequestheader, string validHeaderInRequest, string expectedHeaderName, string expectedId)
        {
            // Arrange
            CorrelationMiddleware middleware = GetCorrelationMiddleware(new CorrelationOptions()
            {
                DefaultHeaderName = defaultHeaderName,
                EnrichLog = true,
                LogPropertyName = "CorrelationId",
                TryToUseRequestHeader = tryToUseRequestheader,
                ValidRequestHeaders = new string[] { validHeaderInRequest },
                WriteCorrelationIDToResponse = writeCorrelationIDToResponse
            });

            ICorrelationService service = GetCorrelationService();

            var context = new HttpContextMock();
            context.SetupRequestHeaders(new Dictionary<string, StringValues>() { { headerNameInRequest, fakeIdFromRequest } });

            ICorrelationIdProvider idProvider = GetCorrelationIdProvider();

            // Act
            await middleware.InvokeAsync(context, service, idProvider);

            // Assert

            if (writeCorrelationIDToResponse)
            {
                context.ResponseMock.HeadersMock.Mock.Verify(d => d.Add(expectedHeaderName, expectedId), Times.Once);
            }
            else
            {
                context.ResponseMock.HeadersMock.Mock.Verify(d => d.Add(It.IsAny<string>(), It.IsAny<StringValues>()), Times.Never);
            }
        }

        [Theory(DisplayName = "Log Should Be Enriched")]
        [MemberData(nameof(GetValuesForLogEnrichment))]
        public async Task LogShouldBeEnriched(bool enrichLog)
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CorrelationMiddleware>>();

            CorrelationMiddleware middleware = GetCorrelationMiddleware(new CorrelationOptions()
            {
                DefaultHeaderName = defaultHeaderName,
                EnrichLog = enrichLog,
                LogPropertyName = "CorrelationId",
                TryToUseRequestHeader = false,
                ValidRequestHeaders = new string[] { headerNameInRequest },
                WriteCorrelationIDToResponse = true
            }, loggerMock.Object);

            ICorrelationService service = GetCorrelationService();

            var context = new HttpContextMock();
            context.SetupRequestHeaders(new Dictionary<string, StringValues>() { { headerNameInRequest, fakeIdFromRequest } });

            ICorrelationIdProvider idProvider = GetCorrelationIdProvider();

            // Act
            await middleware.InvokeAsync(context, service, idProvider);

            // Assert
            Times timesShouldBeCalled = enrichLog ? Times.Once() : Times.Never();

            loggerMock.Verify(m => m.BeginScope<Dictionary<string, object>>(It.IsAny<Dictionary<string, object>>()), timesShouldBeCalled);
        }

        private static CorrelationMiddleware GetCorrelationMiddleware(CorrelationOptions options, ILogger<CorrelationMiddleware>? logger = null)
        {
            RequestDelegate next = (HttpContext _) => Task.CompletedTask;

            CorrelationMiddleware middleware = new CorrelationMiddleware(next, options, logger ?? GetLogger());
            return middleware;
        }

        private static ILogger<CorrelationMiddleware> GetLogger()
        {
            var mock = new Mock<ILogger<CorrelationMiddleware>>();

            return mock.Object;
        }

        private static ICorrelationService GetCorrelationService()
        {
            var mock = new Mock<ICorrelationService>();
            mock.Setup(m => m.GetCurrentCorrelationID()).Returns(fakeId);
            return mock.Object;
        }

        private static ICorrelationIdProvider GetCorrelationIdProvider()
        {
            var mock = new Mock<ICorrelationIdProvider>();
            mock.Setup(m => m.GenerateCorrelationId()).Returns(fakeId);
            return mock.Object;
        }

        public static IEnumerable<object[]> GetValues()
        {
            //Write to response cases
            yield return new object[] { true, true, headerNameInRequest, headerNameInRequest, fakeIdFromRequest };
            yield return new object[] { true, true, "X-Invalid", defaultHeaderName, fakeId };
            yield return new object[] { true, false, headerNameInRequest, defaultHeaderName, fakeId };
            yield return new object[] { true, false, "X-Invalid", defaultHeaderName, fakeId };
            //Don't write to response cases
            yield return new object[] { false, true, headerNameInRequest, "ignored", "ignored" };
            yield return new object[] { false, true, "X-Invalid", "ignored", "ignored" };
            yield return new object[] { false, false, headerNameInRequest, "ignored", "ignored" };
            yield return new object[] { false, false, "X-Invalid", "ignored", "ignored" };
        }

        public static IEnumerable<object[]> GetValuesForLogEnrichment()
        {
            yield return new object[] { true };
            yield return new object[] { false };
        }
    }
}
