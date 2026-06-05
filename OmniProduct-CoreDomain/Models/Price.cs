namespace OmniProduct_CoreDomain.Models;

public class Price
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public decimal Margin { get; set; }     // percentage
    public decimal Vat { get; set; }        // percentage, applied on margin only

    public Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
        Margin = 20;
        Vat = 20;
    }

    public decimal GetResellerPrice()
    {
        var marginAmount = Amount * Margin / 100;
        var vatAmount = marginAmount * Vat / 100;
        return Amount + marginAmount + vatAmount;
    }
}
