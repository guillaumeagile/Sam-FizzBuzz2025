namespace OmniProduct_CoreDomain.Abstractions;

// Identity and display concerns for a catalog entry.
public interface ICatalogItem
{
    string Id { get; set; }
    string Name { get; set; }
    string Slug { get; set; }
    string Status { get; set; }
    string GetDisplayLabel();
}
