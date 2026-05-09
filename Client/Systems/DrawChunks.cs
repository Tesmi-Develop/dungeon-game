using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Systems;
using Hypercube.Core.Viewports;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;

namespace Client.Systems;

public class DrawChunks : PatchEntitySystem
{
    private const bool IsEnabled = false;
    private const int ChunkSize = 64 * 2;
    public override int Priority => -100;

    public override void Draw(IRenderContext renderer, DrawPayload payload)
    {
        if (!IsEnabled)
            return;
        
        DrawChunkGrid(renderer, payload.Camera);
    }
    
    private void DrawChunkGrid(IRenderContext renderer, ICamera camera)
    {
        var halfSize = camera.Size / 2f;
        
        var min = camera.Position.Xy - halfSize;
        var max = camera.Position.Xy + halfSize;
        
        var gridColor = Color.White.WithA(0.2f);
        
        var startX = float.Floor(min.X / ChunkSize) * ChunkSize;
        var startY = float.Floor(min.Y / ChunkSize) * ChunkSize;
        
        for (var x = startX; x <= max.X; x += ChunkSize)
        {
            var start = new Vector2(x, min.Y);
            var end = new Vector2(x, max.Y);
            
            renderer.DrawLine(start, end, gridColor);
        }

        for (var y = startY; y <= max.Y; y += ChunkSize)
        {
            var start = new Vector2(min.X, y);
            var end = new Vector2(max.X, y);
            
            renderer.DrawLine(start, end, gridColor);
        }
        
        for (var x = startX; x < max.X; x += ChunkSize)
        {
            for (var y = startY; y < max.Y; y += ChunkSize)
            {
                var chunkX = (int)float.Floor(x / ChunkSize);
                var chunkY = (int)float.Floor(y / ChunkSize);
                var chunkId = $"[{chunkX}, {chunkY}]";
                var center = new Vector2(x + ChunkSize / 2f, y + ChunkSize / 2f);
            }
        }
    }
}