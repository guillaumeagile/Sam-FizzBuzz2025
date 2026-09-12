namespace OmniProduct_CoreDomain.Models;

public class EmbargoProduct : ProductBase
{
    public override string Status { get; set; } = "UnderEmbargo";
}