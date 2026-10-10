namespace OmniProduct_CoreDomain.Abstractions;

public interface IDentifiable
{
    Ulid Id { get; init; }
}
