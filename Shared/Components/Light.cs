using Hypercube.Ecs.Components;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct Light : IComponent
{
    public static Light Default => new()
    {
        Radius = 200.0f,
        Color = Color.White,
        ColorIntensity = 0.1f,
        IsActive = true,
        Intensity = 1.0f,
        Falloff = 0.3f,
        Offset = Vector2.Zero
    };
    
    public static Light Torch => new()
    {
        Radius = 150.0f,
        Color = Color.White,
        IsActive = true,
        Intensity = 1.2f,
        Falloff = 0.3f,
        Offset = new Vector2(0, 10)
    };
    
    public float Radius = 10;
    public Color Color = Color.White;
    public bool IsActive = true;
    public float Intensity = 1;
    public float ColorIntensity = 0;
    public float Falloff = 0.3f;
    public Vector2 Offset = default;

    public Light()
    {
    }
}