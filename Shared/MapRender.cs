using System.Text.Json;
using DotTiled;
using DotTiled.Layers;
using DotTiled.Properties;
using DotTiled.Serialization;
using DotTiled.Tilesets;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Viewports;
using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Components.MapComponents;
using NetworkTransform = Shared.Components.EngineComponents.NetworkTransform;
using Object = DotTiled.Layers.Objects.Object;

namespace Shared;

public class MapRender : IDisposable
{
    private const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
    private const uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
    private const uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;
    private const uint ROTATED_HEXAGONAL_120_FLAG = 0x10000000;
    private const uint ALL_FLAGS_MASK = FLIPPED_HORIZONTALLY_FLAG | 
                                        FLIPPED_VERTICALLY_FLAG | 
                                        FLIPPED_DIAGONALLY_FLAG | 
                                        ROTATED_HEXAGONAL_120_FLAG;

    private static readonly Dictionary<string, Type> ComponentTypes = [];

    public Map Map { get; private set; } = null!;
    private Dictionary<string, Texture> _textures = new();

    private readonly Loader _loader;
    private readonly ResourcePath _mapPath;

    static MapRender()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(IComponent)) && !type.IsAbstract)
                    ComponentTypes[type.Name] = type;
            }
        }
    }

    public MapRender(ResourcePath mapPath)
    {
        _mapPath = mapPath;
        _loader = Loader.Default();
    }

    // ================================================================== 
    // Loading
    // ==================================================================
    public void Compile(IResourceManager resourceManager)
    {
        Map = _loader.LoadMap($"{AppContext.BaseDirectory}/resources{_mapPath}");

        foreach (var tilesetRef in Map.Tilesets)
        {
            if (tilesetRef.Image.HasValue)
            {
                var source = new ResourcePath(tilesetRef.Source.Value);
                var path = $"{source.ParentDirectory}/{tilesetRef.Image.Value.Source.Value}";

                _textures[tilesetRef.Image.Value.Source.Value] = resourceManager.Load<Texture>($"{_mapPath.ParentDirectory}/{path}");
            }
        }
    }
    
    public Rect2i GetVisibleTileRange(
        ICamera camera, 
        Vector2 mapPosition, 
        Vector2 mapAnchor, 
        Vector2 mapScale,
        int padding = 1)
    {
        var tileW = Map.TileWidth;
        var tileH = Map.TileHeight;

        var mapPixelSize = new Vector2(Map.Width * tileW, Map.Height * tileH);
        var cameraBounds = GetCameraWorldBounds(camera);
        var mapTopLeft = mapPosition - (mapPixelSize * mapScale * mapAnchor);
        
        var localLeft   = (cameraBounds.Left   - mapTopLeft.X) / mapScale.X;
        var localRight  = (cameraBounds.Right  - mapTopLeft.X) / mapScale.X;

        var localTop    = (mapTopLeft.Y + mapPixelSize.Y * mapScale.Y - cameraBounds.Bottom) / mapScale.Y;
        var localBottom = (mapTopLeft.Y + mapPixelSize.Y * mapScale.Y - cameraBounds.Top)    / mapScale.Y;
        
        var minX = (int)Math.Floor(localLeft / tileW);
        var minY = (int)Math.Floor(localTop / tileH);
        var maxX = (int)Math.Ceiling(localRight / tileW);
        var maxY = (int)Math.Ceiling(localBottom / tileH);
        
        minX = Math.Max(0, minX - padding);
        minY = Math.Max(0, minY - padding);
        maxX = Math.Min(Map.Width, maxX + padding);
        maxY = Math.Min(Map.Height, maxY + padding);

        return new Rect2i(minX, minY, maxX, maxY);
    }
    
    // ==================================================================
    // Tile Layers Rendering
    // ==================================================================
    public void Draw(IRenderContext renderContext, ICamera camera, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        var tileSize = new Vector2(Map.TileWidth, Map.TileHeight);
        var mapPixelSize = new Vector2(Map.Width * Map.TileWidth, Map.Height * Map.TileHeight);

        var cameraBounds = GetCameraWorldBounds(camera);

        var visibleRect = GetVisibleTileRange(camera, position, anchor, scale);
        
        foreach (var layer in Map.Layers.OfType<TileLayer>())
        {
            if (!IsLayerVisible(layer)) continue;

            for (var y = visibleRect.Top; y < visibleRect.Bottom; y++)
            {
                for (var x = visibleRect.Left; x < visibleRect.Right; x++)
                {
                    var worldPos = CalculateWorldPosition(position, anchor, scale, mapPixelSize, x * Map.TileWidth, y * Map.TileHeight);
                    
                    var index = y * layer.Width + x;
                    var tileId = layer.GetGlobalTileIDAtCoord(x, y);
                    if (tileId == 0) 
                        continue;
                    
                    var tileset = Map.ResolveTilesetForGlobalTileID(tileId, out var localId);
                    var flags = layer.Data.Value.FlippingFlags.Value[index];
                    var flipH = (flags & FlippingFlags.FlippedHorizontally) != 0;
                    var flipV = (flags & FlippingFlags.FlippedVertically) != 0;
                    var flipD = (flags & FlippingFlags.FlippedDiagonally) != 0;
                    if (!_textures.TryGetValue(tileset.Image.Value.Source.Value, out var texture)) 
                        continue;
                    
                    var column = localId % (uint)tileset.Columns;
                    var row = localId / (uint)tileset.Columns;
                    
                    var uv = CalculateUv(texture.Size, tileSize, column, row);
                    var finalScale = tileSize / texture.Size * scale;
                    
                    var (rotation, signScale) = GetTiledTransformation(flipH, flipV, flipD);
                    finalScale = signScale * finalScale;

                    renderContext.DrawTexture(texture, worldPos, rotation, finalScale, Color.White, uv);
                }
            }
        }
    }
    
    private (Angle Rotation, Vector2 Scale) GetTiledTransformation(bool h, bool v, bool d)
    {
        var rotation = 0;
        var scale = Vector2.One;

        if (d)
        {
            rotation = -90;
            scale = new Vector2(1, -1);
        }

        if (h)
        {
            rotation = -rotation;
            scale = scale.WithX(-scale.X);
        }

        if (v)
        {
            rotation = -rotation;
            scale = scale.WithY(-scale.Y);
        }

        return (Angle.FromDegrees(rotation), scale);
    }

    // ==================================================================
    // Entity Loading (Tile + Object Layers)
    // ==================================================================
    public void Load(World world, PrototypeStorage prototypes, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        var mapPixelSize = new Vector2(Map.Width * Map.TileWidth, Map.Height * Map.TileHeight);

        foreach (var layer in Map.Layers.OfType<TileLayer>())
        {
            for (var y = 0; y < layer.Height; y++)
            {
                for (var x = 0; x < layer.Width; x++)
                {
                    var gid = layer.GetGlobalTileIDAtCoord(x, y);
                    if (gid == 0) continue;

                    var (tileId, _, _, _) = DecomposeGid(gid);
                    var tileset = Map.ResolveTilesetForGlobalTileID(tileId, out var localId);
                    if (tileset == null) continue;
                    
                    var tileDef = tileset.Tiles.FirstOrDefault(tile => tile.ID == localId);
                    if (tileDef == null) continue;

                    if (!tileDef.TryGetProperty("Type", out StringProperty typeName))
                        continue;
                    
                    var worldPos = CalculateWorldPosition(position, anchor, scale, mapPixelSize, 
                                                        x * Map.TileWidth, y * Map.TileHeight);

                    CreateEntityFromTile(
                        world, 
                        tileset, 
                        typeName.Value,
                        tileDef,
                        worldPos,
                        new Vector2(Map.TileWidth, Map.TileHeight) * scale,
                        scale);
                }
            }
        }
        
        foreach (var layer in Map.Layers.OfType<ObjectLayer>())
        {
            foreach (var obj in layer.Objects)
            {
                if (!obj.TryGetProperty("Type", out StringProperty typeName))
                    continue;

                var worldPos = CalculateWorldPosition(position, anchor, scale, mapPixelSize, 
                    obj.X, obj.Y) + (new Vector2(obj.Width, -obj.Height) * scale) / 2;
                CreateEntityFromObject(world, typeName.Value, worldPos, obj, scale);
            }
        }
    }

    private void CreateEntityFromTile(World world, Tileset tileset,
        string typeName, Tile tile, Vector2 worldPosition, Vector2 scaledSize, Vector2 scale)
    {
        CreateEntityBase(world, typeName, worldPosition, scaledSize, scale, tile.GetProperties(), tileset);
    }

    private void CreateEntityFromObject(World world, string typeName,
        Vector2 worldPosition, Object obj, Vector2 scale)
    {
        var size = new Vector2(obj.Width, obj.Height);
        CreateEntityBase(world, typeName, worldPosition, size, scale, obj.GetProperties(), null, obj);
    }

    private void CreateEntityBase(World world, string typeName, 
                                  Vector2 worldPosition, Vector2 size, Vector2 scale, IList<IProperty> sourceProperties, Tileset? tileset = null, Object? obj = null)
    {
        var entity = world.Create();

        world.Add(entity, new NetworkTransform { Position = worldPosition });

        if (tileset != null)
            world.Add(entity, new TilesetRefComponent { Ref = tileset, Size = size });

        world.Add(entity, new MapComponentTag());

        if (obj != null)
            world.Add(entity, new TiledObjectComponent { Object = obj, Scale = scale });

        if (!ComponentTypes.TryGetValue(typeName, out var compType))
            return;
        
        var component = Activator.CreateInstance(compType);
        if (component == null) 
            return;
        
        foreach (var property in sourceProperties)
        {
            if (property.Name == "Type")
                continue;
            
            var fieldInfo = compType.GetField(property.Name);
            fieldInfo?.SetValue(component, property.SourceValue);
        }
        
        world.Add(entity, component);
    }

    private object? ConvertValue(object? value, Type targetType)
    {
        if (value is null) return null;
        if (value is JsonElement json)
            return json.Deserialize(targetType);

        return Convert.ChangeType(value, targetType);
    }

    private static (uint tileId, bool h, bool v, bool d) DecomposeGid(uint gid)
    {
        var tileId = gid & ~ALL_FLAGS_MASK;
        return (tileId,
            (gid & FLIPPED_HORIZONTALLY_FLAG) != 0,
            (gid & FLIPPED_VERTICALLY_FLAG) != 0,
            (gid & FLIPPED_DIAGONALLY_FLAG) != 0);
    }

    private Rect2 CalculateUv(Vector2 texSize, Vector2 tileSize, uint column, uint row)
    {
        var uvTopLeft = new Vector2(column * tileSize.X, (row + 1) * tileSize.Y) / texSize;
        var uvBottomRight = new Vector2((column + 1) * tileSize.X, row * tileSize.Y) / texSize;
        return new Rect2(uvTopLeft, uvBottomRight);
    }

    private Vector2 CalculateWorldPosition(Vector2 position, Vector2 anchor, Vector2 scale, 
                                           Vector2 mapPixelSize, float x, float y)
    {
        var correctedY = Map.Height * Map.TileHeight - y;
        var offset = new Vector2(x, correctedY);
        return position + (offset * scale) - (mapPixelSize * scale * anchor);
    }

    private bool IsLayerVisible(BaseLayer layer)
    {
        var prop = layer.Properties?.FirstOrDefault(p => p.Name == "Visible") as BoolProperty;
        return prop?.Value ?? true;
    }

    private Rect2 GetCameraWorldBounds(ICamera camera)
    {
        var halfWidthWorld = (camera.Size.X * 0.5f) / camera.Scale.X;
        var halfHeightWorld = (camera.Size.Y * 0.5f) / camera.Scale.Y;
        
        var center = camera.Position.Xy;
        
        return Rect2.FromCenter(center, new Vector2(halfWidthWorld * 2, halfHeightWorld * 2));
    }

    public void Dispose() { }
}