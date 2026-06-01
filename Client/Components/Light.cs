using Hypercube.Ecs.Components;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;

namespace Client.Components;

public struct Light : IComponent
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
    
    public float Radius;
    public Color Color;
    public bool IsActive;
    public float Intensity;
    public float ColorIntensity;
    public float Falloff;
    public Vector2 Offset;
}