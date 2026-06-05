namespace OmniProduct_CoreDomain.Ordering;

public abstract class OrderHandler
{
    private readonly OrderHandler _next;

    protected OrderHandler(OrderHandler next)
    {
        _next = next;
    }

    public void Handle(OrderContext context)
    {
        if (!context.CanContinue)
            return;

        Process(context);

        if (context.CanContinue && _next != null)
            _next.Handle(context);
    }

    protected abstract void Process(OrderContext context);
}
