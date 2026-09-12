using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models.Suppliers;

// Table/column annotations copied from Supplier "just to be explicit" - now the two copies
// can (and do) drift, which is exactly how LSP gets broken via inheritance.
[Table("Suppliers")]
public class EuropeanSupplier : Supplier
{
    // Base allows any email up to 256 chars; this redeclaration silently tightens it to 128,
    // so code that works with a Supplier can throw when it's actually a EuropeanSupplier.
    [Required]
    [MaxLength(128)]
    [EmailAddress]
    public new string Email { get; set; }

    public override string RegionCode => "EU";
    public override string GetComplianceLabel() => "VAT";
}
