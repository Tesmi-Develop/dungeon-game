using System.Text.Json;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Shared.Components;
using Shared.Extensions;
using Shared.ResourcesData;
using Shared.ResourcesData.TiledMapParts;
using Shared.ResourcesData.TiledTilesetParts;
using NetworkTransform = Shared.Components.EngineComponents.NetworkTransform;

namespace Shared;

public class MapRender : IDisposable
{
    private const uint FLIPPED_HORIZONTALLY_FLAG  = 0x80000000;
    private const uint FLIPPED_VERTICALLY_FLAG    = 0x40000000;
    private const uint FLIPPED_DIAGONALLY_FLAG    = 0x20000000;
    private const uint ROTATED_HEXAGONAL_120_FLAG = 0x10000000;
    private const uint ALL_FLAGS_MASK = FLIPPED_HORIZONTALLY_FLAG | 
                                        FLIPPED_VERTICALLY_FLAG | 
                                        FLIPPED_DIAGONALLY_FLAG | 
                                        ROTATED_HEXAGONAL_120_FLAG;
    
    private static readonly Dictionary<string, Type> Components = [];
    public TiledMap Map { get; private set; } = null!;
    public List<TiledTileset> Tilesets { get; private set; } = [];
    
    private readonly Dictionary<uint, TiledTileRenderData> _tileDefinitions = new();
    private ResourcePath _path;

    private struct TiledTileRenderData
    {
        public uint RawGid { get; set; }
        public uint TileId { get; set; }
        public TiledTilesetReference Source { get; set; }
        public TiledTileDefinition? TileDefinition { get; set; }
        
        public bool FlipHorizontal { get; set; }
        public bool FlipVertical { get; set; }
        public bool FlipDiagonal { get; set; }
        public bool RotateHex120 { get; set; }
        
        public static TiledTileRenderData FromGid(uint gid, TiledTilesetReference source, TiledTileDefinition? definition)
        {
            return new TiledTileRenderData
            {
                RawGid = gid,
                TileId = gid & ~ALL_FLAGS_MASK,
                Source = source,
                TileDefinition = definition,
                FlipHorizontal = (gid & FLIPPED_HORIZONTALLY_FLAG) != 0,
                FlipVertical = (gid & FLIPPED_VERTICALLY_FLAG) != 0,
                FlipDiagonal = (gid & FLIPPED_DIAGONALLY_FLAG) != 0,
                RotateHex120 = (gid & ROTATED_HEXAGONAL_120_FLAG) != 0
            };
        }
    }
    
    static MapRender()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(IComponent)))
                    Components.Add(type.Name, type);
            }
        }
    }
    
    private struct TiledTileDefinitionRef
    {
        public int Id { get; set; }
        public TiledTilesetReference Source { get; set; }
        public TiledTileDefinition? TileDefinition { get; set; }
    }
    
    public MapRender(ResourcePath tiledMapPath)
    {
        _path = tiledMapPath;
    }
    
        public void Draw(IRenderContext renderContext, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        var tileW = Map.TileWidth;
        var tileH = Map.TileHeight;
        var tileSize = new Vector2(tileW, tileH);
        var mapSize = new Vector2(Map.Width * tileW, Map.Height * tileH);

        foreach (var layer in Map.Layers.Where(l =>
                 {
                     var visibleProperty = l.Properties.Find(e => e.Name == "Visible");
                     if (visibleProperty is null) return true;
                     return visibleProperty.Type == "bool" && visibleProperty.GetValue<bool>();
                 }))
        {
            for (var y = 0; y < layer.Height; y++)
            {
                for (var x = 0; x < layer.Width; x++)
                {
                    var rawGid = (uint)layer.GetTileAt(new Vector2i(x, y));
                    if (rawGid == 0) continue;
                    
                    var tileId = rawGid & ~ALL_FLAGS_MASK;
                    if (!_tileDefinitions.TryGetValue(tileId, out var tileData))
                        continue;

                    var tileset = tileData.Source.Source!;
                    var texture = tileset.Texture!;
                    var localId = tileData.TileId - (uint)tileData.Source.FirstGid;
                    
                    var column = localId % tileset.Columns;
                    var row = localId / tileset.Columns;
                    
                    var texSize = texture.Size;
                    
                    var uvTopLeft = new Vector2(column * tileW, (row + 1) * tileH) / texSize;
                    var uvBottomRight = new Vector2((column + 1) * tileW, row * tileH) / texSize;
                    var uv = new Rect2(uvTopLeft, uvBottomRight);
                    
                    var correctedY = (Map.Height - 1 - y); 
                    var offset = new Vector2(x * tileW, correctedY * tileH);
                    var screenPos = position + (offset * scale) - (mapSize * scale * anchor);
                    
                    var finalScale = tileSize / texture.Size * scale;
                    var rotation = Angle.Zero;
                    var color = Color.White;
                    
                    if (tileData.FlipDiagonal)
                    {
                        uv = new Rect2(
                            new Vector2(uvTopLeft.Y, uvTopLeft.X),
                            new Vector2(uvBottomRight.Y, uvBottomRight.X)
                        );
                        
                        finalScale = new Vector2(finalScale.Y, finalScale.X);
                    }
                    
                    if (tileData.FlipHorizontal)
                    {
                        uv = new Rect2(
                            new Vector2(uv.TopRight.X, uv.TopRight.Y),
                            new Vector2(uv.BottomLeft.X, uv.BottomLeft.Y)
                        );
                    }
                    
                    if (tileData.FlipVertical)
                    {
                        uv = new Rect2(
                            new Vector2(uv.BottomLeft.X, uv.BottomLeft.Y),
                            new Vector2(uv.TopRight.X, uv.TopRight.Y)
                        );
                    }
                    
                    renderContext.DrawTexture(
                        texture,
                        screenPos,
                        rotation,
                        finalScale, 
                        color,
                        uv
                    );
                }
            }
        }
    }

    public void Load(World world, PrototypeStorage prototypes, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        var tileW = Map.TileWidth;
        var tileH = Map.TileHeight;
        var tileSize = new Vector2(tileW, tileH);
        var mapSize = new Vector2(Map.Width * tileW, Map.Height * tileH);

        foreach (var layer in Map.Layers)
        {
            for (var y = 0; y < layer.Height; y++)
            {
                for (var x = 0; x < layer.Width; x++)
                {
                    var gid = layer.GetTileAt(new Vector2i(x, y));
                    if (gid == 0) continue;
                
                    if (!_tileDefinitions.TryGetValue(gid, out var tileDefRef) || tileDefRef.TileDefinition == null)
                        continue;

                    var definition = tileDefRef.TileDefinition;
                    var property = definition.Properties.Find(e => e.Name == "Type");
                    if (property is null || property.Type != "string")
                        continue;
                
                    var name = property.GetValue<string>();

                    var correctedY = (Map.Height - 1 - y); 
                    var offset = new Vector2(x * tileW, correctedY * tileH);
                    
                    var screenPos = position + (offset * scale) - (mapSize * scale * anchor);
                    
                    CreateEntityFromTile(world, prototypes, tileDefRef.Source.Source!, name!, screenPos, tileSize * scale);
                }
            }
        }
    }

    private void CreateEntityFromTile(World world, PrototypeStorage prototypes, TiledTileset tileset, string typeName, Vector2 worldPosition, Vector2 scaledTileSize)
    {
        var entity = world.Create();
        
        world.Add(entity, new NetworkTransform { Position = worldPosition });
        world.Add(entity, new TilesetRefComponent { Ref = tileset, Size = scaledTileSize  });

        if (!prototypes.TryGetPrototype(typeName, out var proto))
            return;

        foreach (var (componentName, properties) in proto)
        {
            if (!Components.TryGetValue(componentName, out var componentType))
                continue;
        
            var component = Activator.CreateInstance(componentType);
            if (component is null) continue;
        
            foreach (var (propName, propValue) in properties)
            {
                var field = componentType.GetField(propName);
                if (field != null)
                {
                    field.SetValue(component, ConvertValue(propValue, field.FieldType));
                    continue;
                }

                var prop = componentType.GetProperty(propName);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(component, ConvertValue(propValue, prop.PropertyType));
                }
            }
            
            world.Add(entity, component); 
        }
    }
    
    private object? ConvertValue(object? value, Type targetType)
    {
        if (value is null) 
            return null;
        
        return (value is JsonElement element ? element : default).Deserialize(targetType);
    }

    public void Compile(IResourceManager resourceManager)
    {
        Map = resourceManager.Load<TiledMap>(_path);

        foreach (var tilesetRef in Map.Tilesets)
        {
            var tilesetPath = _path.ParentDirectory + new ResourcePath(tilesetRef.Path);
            var tileset = resourceManager.Load<TiledTileset>(tilesetPath.WithExtension("tsj"));
            Tilesets.Add(tileset);
            tilesetRef.Source = tileset;

            var texturePath = tilesetPath.ParentDirectory + new ResourcePath(tileset.ImagePath);
            var texture = resourceManager.Load<Texture>(texturePath);
            tileset.Texture = texture;

            for (var i = 0; i < tileset.TileCount; i++)
            {
                var gid = (uint)(i + tilesetRef.FirstGid);
                _tileDefinitions[gid] = TiledTileRenderData.FromGid(
                    gid, 
                    tilesetRef, 
                    tileset.Tiles.Find(r => r.Id == i)
                );
            }
        }
    }

    public void Dispose()
    {
        Map.Dispose();
    }
}