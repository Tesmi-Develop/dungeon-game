using Hypercube.Mathematics.Vectors;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components.Enemies;

[SyncComponent]
public partial struct AttackInfo
{
    public int Damage;
    public Vector2 AttackSize;
    public float MaxTargetRange;
}