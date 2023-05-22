using FluentAssertions;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests;

public class CorrelationServiceUnitTest
{
    [Fact(DisplayName = "CorrelationId should be set as expected")]
    public void CorrelationIdShouldBeSetAsExpected()
    {
        // Arrange
        var service = new CorrelationService();
        string id = "MY-FAKE-ID";

        // Act
        service.SetCorrelationID(id);
        string? settedId = service.GetCurrentCorrelationID();

        // Assert
        settedId.Should().Be(id);
    }

    [Fact(DisplayName = "CorrelationId should be set only once")]
    public void CorrelationIdShouldBeSetOnlyOnce()
    {
        // Arrange
        var service = new CorrelationService();
        string id = "MY-FAKE-ID";
        string id2 = "MY-FAKE-ID-2";

        // Act
        service.SetCorrelationID(id);
        service.SetCorrelationID(id2);
        string? settedId = service.GetCurrentCorrelationID();

        // Assert
        settedId.Should().NotBe(id2);
    }
}