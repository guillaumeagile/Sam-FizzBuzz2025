using AwesomeAssertions;
using OmniProduct_CoreDomain.Errors;
using OmniProduct_CoreDomain.ValueObjects;

namespace OmniProduct_Core.Test;

public class PriceTests
{
    [Fact]
    public void Create_WithValidInput_ShouldReturnPrice()
    {
        var result = Price.Create(100m, "EUR");

        result.IsT0.Should().BeTrue();
        result.AsT0.Amount.Should().Be(100m);
        result.AsT0.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnValidationError()
    {
        var result = Price.Create(-1m, "EUR");

        result.IsT1.Should().BeTrue();
        result.AsT1.Should().BeOfType<ValidationError>();
    }

    [Theory]
    [InlineData("eur")]
    [InlineData("EURO")]
    [InlineData("")]
    public void Create_WithInvalidCurrency_ShouldReturnValidationError(string currency)
    {
        var result = Price.Create(100m, currency);

        result.IsT1.Should().BeTrue();
    }
}
