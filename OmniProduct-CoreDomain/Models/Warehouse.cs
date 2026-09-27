using OmniProduct_CoreDomain.Abstractions;
using OmniProduct_CoreDomain.Models.Storage;

namespace OmniProduct_CoreDomain.Models;

public class Warehouse : IDentifiable
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public string Region { get; set; }

    public List<StoredProduct> StoredProducts { get; set; } = new();
}
