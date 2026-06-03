using System.Text.Json;
using DotTiled;
using DotTiled.Serialization;
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
using NetworkTransform = Shared.Components.EngineComponents.NetworkTransform;
using Object = DotTiled.Object;

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

    // ==================================================================
    // Tile Layers Rendering
    // ==================================================================
    public void Draw(IRenderContext renderContext, ICamera camera, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        var tileSize = new Vector2(Map.TileWidth, Map.TileHeight);
        var mapPixelSize = new Vector2(Map.Width * Map.TileWidth, Map.Height * Map.TileHeight);

        var cameraBounds = GetCameraWorldBounds(camera);
        var cullingBounds = cameraBounds.Inflate(new Vector2(20, 20));

        foreach (var layer in Map.Layers.OfType<TileLayer>())
        {
            if (!IsLayerVisible(layer)) continue;

            for (int y = 0; y < layer.Height; y++)
            {
                for (int x = 0; x < layer.Width; x++)
                {
                    var spriteSizePixels = new Vector2(Map.TileWidth * scale.X, Map.TileHeight * scale.Y);
                    var worldPos = CalculateWorldPosition(position, anchor, scale, mapPixelSize, x * Map.TileWidth, y * Map.TileHeight);
                    var spriteBounds = Rect2.FromCenter(worldPos, spriteSizePixels);
                    if (!spriteBounds.Intersects(cullingBounds))
                        continue;
                    
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
            for (int y = 0; y < layer.Height; y++)
            {
                for (int x = 0; x < layer.Width; x++)
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

                    CreateEntityFromTile(world, prototypes, tileset, typeName.Value, worldPos, 
                                       new Vector2(Map.TileWidth, Map.TileHeight) * scale);
                }
            }
        }

        // 2. Object Layers
        foreach (var layer in Map.Layers.OfType<ObjectLayer>())
        {
            if (!IsLayerVisible(layer)) continue;

            foreach (var obj in layer.Objects)
            {
                if (!obj.TryGetProperty("Type", out StringProperty typeName))
                    continue;

                var worldPos = CalculateObjectWorldPosition(position, anchor, scale, obj);
                CreateEntityFromObject(world, prototypes, typeName.Value, worldPos, obj);
            }
        }
    }
    
    private Vector2 CalculateObjectWorldPosition(Vector2 position, Vector2 anchor, Vector2 scale, Object obj)
    {
        var mapHeightPixels = Map.Height * Map.TileHeight;
        var worldY = mapHeightPixels - obj.Y; // Flip Y (Tiled -> world)

        var mapPixelSize = new Vector2(Map.Width * Map.TileWidth, mapHeightPixels);
        var offset = new Vector2(obj.X, worldY);

        return position + (offset * scale) - (mapPixelSize * scale * anchor);
    }

    private void CreateEntityFromTile(World world, PrototypeStorage prototypes, Tileset tileset, 
                                      string typeName, Vector2 worldPosition, Vector2 scaledSize)
    {
        CreateEntityBase(world, prototypes, typeName, worldPosition, scaledSize, tileset);
    }

    private void CreateEntityFromObject(World world, PrototypeStorage prototypes, string typeName, 
                                        Vector2 worldPosition, Object obj)
    {
        var size = new Vector2(obj.Width, obj.Height);
        CreateEntityBase(world, prototypes, typeName, worldPosition, size, null, obj);
    }

    private void CreateEntityBase(World world, PrototypeStorage prototypes, string typeName, 
                                  Vector2 worldPosition, Vector2 size, Tileset? tileset = null, Object? obj = null)
    {
        var entity = world.Create();

        world.Add(entity, new NetworkTransform { Position = worldPosition });

        if (tileset != null)
            world.Add(entity, new TilesetRefComponent { Ref = tileset, Size = size });

        world.Add(entity, new MapComponentTag());

        if (obj != null) {}
            //world.Add(entity, new TiledObjectComponent { Object = obj });

        if (!prototypes.TryGetPrototype(typeName, out var prototype))
            return;

        foreach (var (compName, properties) in prototype)
        {
            if (!ComponentTypes.TryGetValue(compName, out var compType))
                continue;

            var component = Activator.CreateInstance(compType);
            if (component == null) continue;

            foreach (var (propName, value) in properties)
            {
                var field = compType.GetField(propName);
                if (field != null)
                {
                    field.SetValue(component, ConvertValue(value, field.FieldType));
                    continue;
                }

                var prop = compType.GetProperty(propName);
                if (prop?.CanWrite == true)
                    prop.SetValue(component, ConvertValue(value, prop.PropertyType));
            }

            world.Add(entity, component);
        }
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

    private Vector2 CalculateFinalScale(Vector2 tileSize, Vector2 texSize, Vector2 scale, 
                                        bool h, bool v, bool d, out Angle rotation)
    {
        rotation = Angle.Zero;
        var s = Vector2.One;

        if (d)
        {
            rotation = Angle.FromDegrees(-90);
            s = new Vector2(1, -1);
        }

        if (h) { rotation = -rotation; s = s.WithX(-s.X); }
        if (v) { rotation = -rotation; s = s.WithY(-s.Y); }

        return s * (tileSize / texSize * scale);
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