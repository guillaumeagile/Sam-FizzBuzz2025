using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniProduct_CoreDomain.Models;

[Table("Warehouses")]
public class Warehouse
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(512)]
    public string Address { get; set; }

    [MaxLength(8)]
    public string Region { get; set; }

    [NotMapped]
    public List<ActiveProduct> Products { get; set; } = new();
}
