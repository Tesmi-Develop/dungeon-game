using System.Text.Json;
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
    public string Name => _path;

    private struct TiledTileRenderData
    {
        public uint TileId { get; set; }
        public TiledTilesetReference Source { get; set; }
        public TiledTileDefinition? TileDefinition { get; set; }
        
        public static TiledTileRenderData FromStaticData(uint tileId, TiledTilesetReference source, TiledTileDefinition? definition)
        {
            return new TiledTileRenderData
            {
                TileId = tileId,
                Source = source,
                TileDefinition = definition
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
    
    public (Angle Rotation, Vector2 Scale) GetTiledTransformation(bool h, bool v, bool d)
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
    
    public void Draw(IRenderContext renderContext, ICamera camera, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        var tileW = Map.TileWidth;
        var tileH = Map.TileHeight;
        var tileSize = new Vector2(tileW, tileH);
        var mapSize = new Vector2(Map.Width * tileW, Map.Height * tileH);

        const float padding = 10.0f;
        
        var cameraBounds = GetCameraWorldBounds(camera);
        var cullingBounds = cameraBounds.Inflate(new Vector2(padding, padding));
        
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

                    var inv = ~ALL_FLAGS_MASK;
                    var tileId = rawGid & inv;
                    if (!_tileDefinitions.TryGetValue(tileId, out var tileData))
                        continue;
                    
                    bool flipH = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
                    bool flipV = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
                    bool flipD = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;
                    
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
                    
                    var (rotation, signScale) = GetTiledTransformation(flipH, flipV, flipD);
                    finalScale = signScale * finalScale;
                    
                    if (tileData.TileDefinition != null)
                    {
                        var rotationProp = tileData.TileDefinition.Properties.Find(p => p.Name == "Rotation");
                        if (rotationProp != null && rotationProp.Type == "float")
                        {
                            rotation += Angle.FromDegrees(rotationProp.GetValue<float>());
                        }
                    }
                    
                    var spriteSizePixels = new Vector2(tileW * scale.X, tileH * scale.Y);
                    var spriteBounds = Rect2.FromCenter(screenPos, spriteSizePixels);
                    
                    if (!spriteBounds.Intersects(cullingBounds))
                        continue;
                    
                    var color = Color.White;

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
        
    private Rect2 GetCameraWorldBounds(ICamera camera)
    {
        var halfWidthWorld = (camera.Size.X * 0.5f) / camera.Scale.X;
        var halfHeightWorld = (camera.Size.Y * 0.5f) / camera.Scale.Y;
        
        var center = camera.Position.Xy;
        
        return Rect2.FromCenter(center, new Vector2(halfWidthWorld * 2, halfHeightWorld * 2));
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
        world.Add(entity, new MapComponentTag());

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

            // Заполняем словарь статическими данными. 
            // Ключом является ЧИСТЫЙ ID тайла (без флагов), так как флаги зависят от размещения на карте.
            for (var i = 0; i < tileset.TileCount; i++)
            {
                var pureTileId = (uint)(i + tilesetRef.FirstGid);
                
                _tileDefinitions[pureTileId] = TiledTileRenderData.FromStaticData(
                    pureTileId, 
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