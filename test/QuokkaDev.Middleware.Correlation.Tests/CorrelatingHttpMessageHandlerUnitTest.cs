using FluentAssertions;
using HttpContextMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class CorrelatingHttpMessageHandlerUnitTest
    {
        public CorrelatingHttpMessageHandlerUnitTest()
        {
        }

        [Fact(DisplayName = "Null Request Should Throw Exception")]
        public async Task NullRequestShouldThrowException()
        {
            // Arrange
            CorrelationOptions options = new() { DefaultHeaderName = "foo" };

            var mockOptions = new Mock<IOptions<CorrelationOptions>>();
            mockOptions.Setup(m => m.Value).Returns(options);

            var mockAccessor = new Mock<IHttpContextAccessor>();

            HttpRequestMessage? request = null;

            var handler = new CorrelatingHttpMessageHandlerWrapper(mockOptions.Object, mockAccessor.Object);

            // Act
            var execution = async () => await handler.WrappedSendAsync(request!, CancellationToken.None);

            // Assert
            await execution.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
        }

        [Theory(DisplayName = "CorrelationId Shoul Be Forwarded As Expected")]
        [MemberData(nameof(GetForwardData))]
        public async Task CorrelationIdShoulBeForwardedAsExpected(bool isCorrelationIdSet, bool isHeaderAlreadySet, bool shouldBeForwarded)
        {
            // Arrange
            CorrelationOptions options = new() { DefaultHeaderName = "foo" };

            var mockOptions = new Mock<IOptions<CorrelationOptions>>();
            mockOptions.Setup(m => m.Value).Returns(options);

            var correlationServiceMock = new Mock<ICorrelationService>();
            correlationServiceMock.Setup(m => m.GetCurrentCorrelationID()).Returns(isCorrelationIdSet ? "MY-FAKE-CORRELATION-ID" : (string)null!);

            HttpContextMock context = new HttpContextMock();
            context.RequestServicesMock.Mock.Setup(m => m.GetService(typeof(ICorrelationService))).Returns(correlationServiceMock.Object);

            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(m => m.HttpContext).Returns((HttpContext)context);

            HttpRequestMessage request = new();
            if (isHeaderAlreadySet)
            {
                request.Headers.Add("foo", "MY-FAKE-CORRELATION-ID-ALREADY-SET");
            }

            Mock<ICorrelationForwarder> correlationForwarderMock = new Mock<ICorrelationForwarder>();

            var handler = new CorrelatingHttpMessageHandlerWrapper(mockOptions.Object, new MockResponseHandler(), mockAccessor.Object, correlationForwarderMock.Object);

            // Act
            await handler.WrappedSendAsync(request, CancellationToken.None);

            // Assert
            correlationForwarderMock.Verify(m => m.Forward(It.IsAny<string>()), shouldBeForwarded ? Times.Once : Times.Never);
        }

        public static IEnumerable<object[]> GetForwardData()
        {
            yield return new object[] { true, true, false };
            yield return new object[] { true, false, true };
            yield return new object[] { false, true, false };
            yield return new object[] { false, false, false };
        }
    }

    public class CorrelatingHttpMessageHandlerWrapper : CorrelatingHttpMessageHandler
    {
        public CorrelatingHttpMessageHandlerWrapper(IOptions<CorrelationOptions> options, IHttpContextAccessor accessor, ICorrelationForwarder? forwarder = null)
            : base(options, accessor, forwarder)
        {
        }

        public CorrelatingHttpMessageHandlerWrapper(IOptions<CorrelationOptions> options, HttpMessageHandler innerHandler, IHttpContextAccessor accessor, ICorrelationForwarder? forwarder = null)
            : base(options, innerHandler, accessor, forwarder)
        {
        }

        public Task<HttpResponseMessage> WrappedSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsync(request, cancellationToken);
        }
    }

    public class MockResponseHandler : DelegatingHandler
    {
        public MockResponseHandler()
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}
