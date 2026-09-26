using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models.Suppliers;

[Table("Suppliers")]
public class AsiaSupplier : Supplier
{
    // Base allows names up to 256 chars; this redeclaration shortens the limit to 64,
    // another silent narrowing that breaks substitutability for anything already
    // relying on the base contract's 256-char allowance.
    [Required]
    [MaxLength(64)]
    public new string Name { get; set; }

    public override string RegionCode => "ASIA";
    public override string GetComplianceLabel() => "GST";
}
