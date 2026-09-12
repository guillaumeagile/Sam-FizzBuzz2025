namespace OmniProduct_CoreDomain.Abstractions;

// Media concerns: images keyed by display context (thumbnail, hero, etc).
public interface IHasImages
{
    Dictionary<string, string> Images { get; set; }
    void AddImage(string context, string url);
}
