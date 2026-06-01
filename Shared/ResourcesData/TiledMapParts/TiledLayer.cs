using Hypercube.Mathematics.Vectors;
using Shared.ResourcesData.TiledTilesetParts;

namespace Shared.ResourcesData.TiledMapParts;

public class TiledLayer
{
    public uint[] Data { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Visible { get; set; }
    public List<Property> Properties { get; set; } = [];

    public uint GetTileAt(Vector2i point)
    {
        if (point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height)
            return 0;

        return Data[point.Y * Width + point.X];
    }
}