namespace OmniProduct_CoreDomain.Models;

public class DeprecatedProduct : ProductBase
{
    public override string Status { get; set; } = "deprecated";
}
