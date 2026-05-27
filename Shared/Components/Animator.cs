using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;
using Shared.ResourcesData;

namespace Shared.Components;

[SyncComponent]
public partial struct Animator : IComponent
{
    [NonSynced]
    public AnimationClip? CurrentClip = null;
    [NonSynced]
    public int PrevFrameIndex = -1;
    [NonSynced] 
    public List<EventData> CurrentEvents = [];
    [NonSynced]
    public bool ActiveEvents = false;
    public string CurrentClipName = string.Empty;
    public bool IsPlaying = false;
    public bool IsPaused = false;
    public bool IsLooping = false;
    public int StartTick = 0;
    public Vector2 Scale = Vector2.One;
    public int PausedTicks = 0;

    public Animator()
    {
    }
}