using FluentAssertions;
using System;
using Xunit;

namespace QuokkaDev.Middleware.Correlation.Tests
{
    public class GuidCorrelationIdProviderUnitTest
    {
        public GuidCorrelationIdProviderUnitTest()
        {
        }

        [Fact(DisplayName = "Non Empty Guid Should Be Generated")]
        public void NonEmptyGuidShouldBeGenerated()
        {
            // Arrange
            GuidCorrelationIdProvider provider = new GuidCorrelationIdProvider();

            // Act
            string guid = provider.GenerateCorrelationId();

            // Assert
            guid.Should().NotBe(Guid.Empty.ToString());
        }

        [Fact(DisplayName = "Non Empty String Should Be Generated")]
        public void NonEmptyStringBeGenerated()
        {
            // Arrange
            GuidCorrelationIdProvider provider = new GuidCorrelationIdProvider();

            // Act
            string guid = provider.GenerateCorrelationId();

            // Assert
            guid.Should().NotBeEmpty(because: "Empty string is not a valid correlation id");
            guid.Should().NotBeNull(because: "Null is not a valid correlation id");
        }

        [Fact(DisplayName = "A Guid Should Be Generated")]
        public void AGuidShouldBeGenerated()
        {
            // Arrange
            GuidCorrelationIdProvider provider = new GuidCorrelationIdProvider();

            // Act
            string guid = provider.GenerateCorrelationId();

            // Assert
            guid.Should().MatchRegex("^[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}$");
        }
    }
}
