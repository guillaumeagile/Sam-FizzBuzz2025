using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models.Suppliers;

[Table("Suppliers")]
public class AmericasSupplier : Supplier
{
    // No overridden field constraints here - kept loose on purpose so the four subclasses
    // don't even agree with each other on which fields tighten, let alone with the base.
    public override string RegionCode => "AMERICAS";
    public override string GetComplianceLabel() => "Sales Tax";
}
