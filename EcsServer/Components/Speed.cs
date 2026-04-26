using EcsServer.Attributes;

namespace EcsServer.Components;

[SyncComponent]
public partial struct Speed
{
    public float Value;
}