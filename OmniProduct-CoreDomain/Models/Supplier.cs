using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OmniProduct_CoreDomain.Abstractions;

namespace OmniProduct_CoreDomain.Models;

[Table("Suppliers")]
public class Supplier : IDentifiable
{
    [Key]
    public string Id { get; set; }

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
}
