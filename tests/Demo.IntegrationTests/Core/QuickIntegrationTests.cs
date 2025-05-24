using FluentAssertions;
using Xunit;

namespace Demo.IntegrationTests.Core;

public class QuickIntegrationTests
{
    [Fact]
    public void Integration_Test_Framework_Should_Be_Working()
    {
        // Arrange
        var expected = "Integration tests are working";
        
        // Act
        var actual = "Integration tests are working";
        
        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void FluentAssertions_Should_Be_Available()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };
        
        // Act & Assert
        list.Should().HaveCount(3);
        list.Should().Contain("item2");
        list.Should().NotContain("item4");
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void Parameterized_Tests_Should_Work(int a, int b, int expected)
    {
        // Act
        var result = a + b;
        
        // Assert
        result.Should().Be(expected);
    }
}