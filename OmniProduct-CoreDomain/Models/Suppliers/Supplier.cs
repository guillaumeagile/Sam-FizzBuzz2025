using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models.Suppliers;

[Table("Suppliers")]
public abstract class Supplier
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; }

    [MaxLength(8)]
    public string Region { get; set; }

    // EF navigation back to products that picked this supplier for a region. Never populated
    // manually because ProductService keeps its own separate List<Supplier> as the source of truth.
    [NotMapped]
    public List<Product> Products { get; set; } = new();

    // Every subclass must supply a region code; base behaviour never assumed otherwise, so
    // overriding this can't strengthen preconditions or weaken postconditions - safe per LSP.
    public abstract string RegionCode { get; }

    // Default label callers can already rely on; subclasses only ever narrow it to a more
    // specific (still non-null, still a string) label, never break the base contract.
    public virtual string GetComplianceLabel() => "N/A";
}
