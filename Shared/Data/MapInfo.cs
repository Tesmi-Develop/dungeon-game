using System.Text.Json.Serialization;

namespace Shared.Data;

public class MapInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("path")]
    public string Path { get; set; }
    
    [JsonPropertyName("scale")]
    public int Scale { get; set; }
}