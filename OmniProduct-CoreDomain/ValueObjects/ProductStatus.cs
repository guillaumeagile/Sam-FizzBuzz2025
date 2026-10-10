namespace OmniProduct_CoreDomain.ValueObjects;

// Lifecycle states of a Product. Used as the alternatives of Product.Status (a OneOf union), so the
// compiler checks every place that handles a status.
public record Active;

public record OutOfStock;

public record Deprecated;
