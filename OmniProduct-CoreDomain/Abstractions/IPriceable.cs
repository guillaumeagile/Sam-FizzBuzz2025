using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Abstractions;

// Pricing concerns: current price and reseller-facing calculations.
public interface IPriceable
{
    Price Price { get; set; }
    decimal GetResellerPrice();
    void SetMargin(decimal marginPercent);
}
