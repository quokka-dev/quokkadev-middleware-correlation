using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class CorrelationExtensionsUnitTest
    {
        public CorrelationExtensionsUnitTest()
        {
        }

        [Fact(DisplayName = "Middleware Should Be Added To Pipeline")]
        public void MiddlewareShouldBeAddedToPipeline()
        {
            // Arrange
            var mock = new Mock<IApplicationBuilder>();
            var builder = mock.Object;

            // Act
            builder.UseCorrelation();

            // Assert
            mock.Verify(m => m.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }

        [Fact(DisplayName = "Add Correlation Should Register Default Services")]
        public void AddCorrelationShouldRegisterDefaultServices()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddCorrelation();

            // Assert
            var correlationServiceRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationService));
            correlationServiceRegistration.Should().NotBeNull();
            correlationServiceRegistration!.ImplementationType!.FullName.Should().Be(typeof(CorrelationService).FullName);
            correlationServiceRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var correlationIdProviderRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationIdProvider));
            correlationIdProviderRegistration.Should().NotBeNull();
            correlationIdProviderRegistration!.ImplementationType!.FullName.Should().Be(typeof(GuidCorrelationIdProvider).FullName);
            correlationIdProviderRegistration.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact(DisplayName = "Add Correlation With Service Should Register right Services")]
        public void AddCorrelationWithServiceShouldRegisterRightServices()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddCorrelationWithService<FakeCorrelationService>();

            // Assert
            var correlationServiceRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationService));
            correlationServiceRegistration.Should().NotBeNull();
            correlationServiceRegistration!.ImplementationType!.FullName.Should().Be(typeof(FakeCorrelationService).FullName);
            correlationServiceRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var correlationIdProviderRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationIdProvider));
            correlationIdProviderRegistration.Should().NotBeNull();
            correlationIdProviderRegistration!.ImplementationType!.FullName.Should().Be(typeof(GuidCorrelationIdProvider).FullName);
            correlationIdProviderRegistration.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact(DisplayName = "Add Correlation With Service Should Register right Services")]
        public void AddCorrelationWithProviderShouldRegisterRightServices()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddCorrelationWithProvider<FakeCorrelationIdProvider>();

            // Assert
            var correlationServiceRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationService));
            correlationServiceRegistration.Should().NotBeNull();
            correlationServiceRegistration!.ImplementationType!.FullName.Should().Be(typeof(CorrelationService).FullName);
            correlationServiceRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var correlationIdProviderRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationIdProvider));
            correlationIdProviderRegistration.Should().NotBeNull();
            correlationIdProviderRegistration!.ImplementationType!.FullName.Should().Be(typeof(FakeCorrelationIdProvider).FullName);
            correlationIdProviderRegistration.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact(DisplayName = "Add Correlation Should Register Right Services")]
        public void AddCorrelationShouldRegisterRightServices()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddCorrelation<FakeCorrelationService, FakeCorrelationIdProvider>();

            // Assert
            var correlationServiceRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationService));
            correlationServiceRegistration.Should().NotBeNull();
            correlationServiceRegistration!.ImplementationType!.FullName.Should().Be(typeof(FakeCorrelationService).FullName);
            correlationServiceRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var correlationIdProviderRegistration = services.FirstOrDefault(r => r.ServiceType == typeof(ICorrelationIdProvider));
            correlationIdProviderRegistration.Should().NotBeNull();
            correlationIdProviderRegistration!.ImplementationType!.FullName.Should().Be(typeof(FakeCorrelationIdProvider).FullName);
            correlationIdProviderRegistration.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact(DisplayName = "Null Builder Should Be Throw Exception")]
        public void NullBuilderShouldBeThrowException()
        {
            // Arrange
            IHttpClientBuilder? builder = null;

            // Act
            var execution = () => builder!.ForwardCorrelationId();

            // Assert
            execution.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        }
    }

    internal class FakeCorrelationService : ICorrelationService
    {
        public string? GetCurrentCorrelationID()
        {
            throw new NotImplementedException();
        }

        public void SetCorrelationID(string id)
        {
            throw new NotImplementedException();
        }
    }

    internal class FakeCorrelationIdProvider : ICorrelationIdProvider
    {
        public string GenerateCorrelationId()
        {
            throw new NotImplementedException();
        }
    }
}
