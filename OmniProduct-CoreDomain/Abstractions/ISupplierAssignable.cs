using OmniProduct_CoreDomain.Models;

namespace OmniProduct_CoreDomain.Abstractions;

// Sourcing concerns: which supplier serves which region for this item.
public interface ISupplierAssignable
{
    Dictionary<string, Supplier> SuppliersRegions { get; set; }
    void AddSupplierToRegion(string region, List<Supplier> suppliers);
}
