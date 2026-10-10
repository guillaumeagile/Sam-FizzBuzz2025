using OmniProduct_CoreDomain.Errors;
using OneOf;

namespace OmniProduct_CoreDomain.ValueObjects;

public record Price
{
    public decimal Amount { get;   }

    public string Currency { get;  }

    public decimal Margin { get; internal init; }     // percentage

    public decimal Vat { get;   }        // percentage, applied on margin only

    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
        Margin = 20;
        Vat = 20;
    }

    // Input is validated here, at the boundary, so an invalid Price cannot exist.
    public static OneOf<Price, ValidationError> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return new ValidationError("Amount must not be negative.");

        if (currency.Length != 3 || !currency.All(char.IsAsciiLetterUpper))
            return new ValidationError($"Currency '{currency}' must be a 3-letter ISO code.");

        return new Price(amount, currency);
    }

    public decimal GetResellerPrice()
    {
        var marginAmount = Amount * Margin / 100;
        var vatAmount = marginAmount * Vat / 100;
        return Amount + marginAmount + vatAmount;
    }
}
