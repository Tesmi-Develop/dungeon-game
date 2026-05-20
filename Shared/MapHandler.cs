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

public class MapHandler : IDisposable
{
    private static readonly Dictionary<string, Type> Components = [];
    public TiledMap Map { get; private set; } = null!;
    public List<TiledTileset> Tilesets { get; private set; } = [];
    
    private readonly Dictionary<int, TiledTileDefinitionRef> _tileDefinitions = new();
    private ResourcePath _path;

    static MapHandler()
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
    
    public MapHandler(ResourcePath tiledMapPath)
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
                     
                     if (visibleProperty is null) 
                         return true;
                     
                     return visibleProperty.Type == "bool" && visibleProperty.GetValue<bool>();
                 }))
        {
            for (var y = 0; y < layer.Height; y++)
            {
                for (var x = 0; x < layer.Width; x++)
                {
                    var gid = layer.GetTileAt(new Vector2i(x, y));
                    if (gid == 0) 
                        continue;
                    
                    if (!_tileDefinitions.TryGetValue(gid, out var tileDefRef))
                        continue;

                    var tileset = tileDefRef.Source.Source!;
                    var texture = tileset.Texture!;
                    var localId = gid - tileDefRef.Source.FirstGid;
                    
                    var column = localId % tileset.Columns;
                    var row = localId / tileset.Columns;
                    
                    var texSize = texture.Size;
                    
                    var uvTopLeft = new Vector2(column * tileW, (row + 1) * tileH) / texSize;
                    var uvBottomRight = new Vector2((column + 1) * tileW, row * tileH) / texSize;
                    
                    var uv = new Rect2(uvTopLeft, uvBottomRight);
                    
                    var correctedY = (Map.Height - 1 - y); 
                    var offset = new Vector2(x * tileW, correctedY * tileH);
                    
                    var screenPos = position + (offset * scale) - (mapSize * scale * anchor);
                    
                    renderContext.DrawTexture(
                        texture,
                        screenPos,
                        Angle.Zero,
                        tileSize / texture.Size * scale, 
                        Color.White,
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
                _tileDefinitions[i + tilesetRef.FirstGid] = new TiledTileDefinitionRef
                {
                    Id = i,
                    Source = tilesetRef,
                    TileDefinition = tileset.Tiles.Find(r => r.Id == i)
                };
            }
        }
    }

    public void Dispose()
    {
        Map.Dispose();
    }
}