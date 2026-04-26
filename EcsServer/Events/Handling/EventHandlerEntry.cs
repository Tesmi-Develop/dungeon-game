namespace EcsServer.Events.Handling;

public readonly struct HandlerEntry
{
    public readonly int Priority;
    public readonly object Handler;

    public HandlerEntry(int priority, object handler)
    {
        Priority = priority;
        Handler = handler;
    }
}
