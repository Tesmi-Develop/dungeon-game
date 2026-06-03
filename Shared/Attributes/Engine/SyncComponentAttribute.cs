namespace Shared.Attributes.Engine;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class SyncComponentAttribute : Attribute
{
    public readonly bool InvokeEventWhenDirty = false;
    
    public SyncComponentAttribute()
    {
        
    }

    public SyncComponentAttribute(bool invokeEventWhenDirty = false)
    {
        InvokeEventWhenDirty = invokeEventWhenDirty;
    }
}
