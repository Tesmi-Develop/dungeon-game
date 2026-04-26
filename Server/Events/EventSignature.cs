namespace Server.Events;

public readonly struct EventSignature : IEquatable<EventSignature>
{
    public readonly Type Component;
    public readonly Type Event;

    public EventSignature(Type component, Type @event)
    {
        Component = component;
        Event = @event;
    }

    public bool Equals(EventSignature other)
        => Component == other.Component && Event == other.Event;

    public override bool Equals(object? obj)
        => obj is EventSignature other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Component, Event);

    public static bool operator ==(EventSignature left, EventSignature right)
        => left.Equals(right);

    public static bool operator !=(EventSignature left, EventSignature right)
        => !(left == right);
}