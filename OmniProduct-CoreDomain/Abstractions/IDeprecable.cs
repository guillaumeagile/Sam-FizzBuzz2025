namespace OmniProduct_CoreDomain.Abstractions;

// Lifecycle concerns: retiring a catalog entry.
public interface IDeprecable
{
    void Deprecate();
}
