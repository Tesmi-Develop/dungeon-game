namespace EcsServer;

[AttributeUsage(AttributeTargets.Class)]
public class EcsSystemAttribute : Attribute
{
    public readonly int Priority;
    
    public EcsSystemAttribute() : this(EcsPriority.Default) {}
    
    public EcsSystemAttribute(int priority)
    {
        Priority = priority;
    }

    public EcsSystemAttribute(EcsPriority priority) : this((int)priority) {}
}