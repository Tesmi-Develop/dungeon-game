using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.ResourcesData;

namespace Client.Components.AnimationComponents;

public struct Animator : IComponent
{
    public AnimationClip? CurrentClip = null;
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