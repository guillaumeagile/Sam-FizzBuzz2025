namespace OmniProduct_CoreDomain.Models;

public class OutOfStockProduct : ProductBase
{
    public override string Status { get; set; } = "out_of_stock";
}
