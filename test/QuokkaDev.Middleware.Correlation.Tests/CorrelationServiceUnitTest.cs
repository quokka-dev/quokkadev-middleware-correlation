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
        const string id = "MY-FAKE-ID";

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
        const string id = "MY-FAKE-ID";
        const string id2 = "MY-FAKE-ID-2";

        // Act
        service.SetCorrelationID(id);
        service.SetCorrelationID(id2);
        string? idSet = service.GetCurrentCorrelationID();

        // Assert
        idSet.Should().NotBe(id2);
    }
}