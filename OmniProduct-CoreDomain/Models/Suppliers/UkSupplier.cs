using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models.Suppliers;

[Table("Suppliers")]
public class UkSupplier : Supplier
{
    // Base makes Region optional (no [Required]); this redeclaration makes it mandatory,
    // so a caller that legally builds a base-typed Supplier without a Region breaks here.
    [Required]
    [MaxLength(8)]
    public new string Region { get; set; }

    public override string RegionCode => "UK";
    public override string GetComplianceLabel() => "UK VAT";
}
