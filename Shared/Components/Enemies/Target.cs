

using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components.Enemies;

[SyncComponent]
public partial struct Target : IComponent
{
    public long? EntityMask;
    
    [NonSynced]
    public Entity? TargetEntity;

    public int TargetAcquisitionRadius;
    public int TargetRetentionRadius;
}