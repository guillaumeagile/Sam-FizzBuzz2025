using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models;

// Not a real EF entity (it's [NotMapped] on Product, flattened into PriceAmount/PriceCurrency/...)
// but it kept its data annotations from when someone tried to map it directly and gave up.
[Table("Prices")]
public class Price
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(8)]
    public string Currency { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Margin { get; set; }     // percentage

    [Column(TypeName = "decimal(5,2)")]
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
