namespace OmniProduct_CoreDomain.Abstractions;

// Discount concerns: promotional codes attached to a catalog entry.
public interface IDiscountable
{
    List<string> Discounts { get; set; }
    void AddDiscount(string discountCode);
}
