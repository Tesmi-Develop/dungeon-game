using System.Text.Json.Serialization;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources.Loaders;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;

namespace Shared.ResourcesData;

public sealed class AnimationClip : Resource
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("loop")]
    public bool Loop { get; set; }
    
    [JsonPropertyName("image")]
    public string Image { get; set; }
    
    [JsonPropertyName("duration")]
    public float Duration { get; set; }
    
    [JsonPropertyName("frameSize")]
    public FrameSize FrameSize { get; set; }
    
    [JsonPropertyName("grid")]
    public Grid Grid { get; set; }

    [JsonPropertyName("events")] 
    public EventData[] Events { get; set; } = [];
    
    [JsonIgnore]
    public Dictionary<int, List<EventData>> EventsByFrameIndex = new();
    
    [JsonIgnore]
    public int TicksPerFrame { get; set; }
    [JsonIgnore]
    public Vector2i TextureSize { get; set; }
    [JsonIgnore]
    public Rect2[] Frames { get; set; } = [];
    [JsonIgnore]
    public Texture? Texture { get; set; }

    public override void Dispose()
    {
        
    }
}

public record FrameSize(int Width, int Height);
public record Grid(int Width, int Height);

public class EventData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("frameIndex")] 
    public int FrameIndex { get; set; } = 0;

    [JsonPropertyName("data")] 
    public string Data { get; set; } = string.Empty;
}