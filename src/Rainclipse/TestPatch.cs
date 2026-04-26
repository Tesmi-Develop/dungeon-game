using System.Text;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Rendering.Manager;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Input.Handler;
using Hypercube.Core.Resources;
using Hypercube.Core.Viewports;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Matrices;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;

namespace Rainclipse;

public sealed class TestPatch : Patch, IPostInject
{
    private const int ChunkSize = 256;
    
    [Dependency] private readonly IRenderManager _render = null!;
    [Dependency] private readonly IResourceManager _resource = null!;
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    [Dependency] private readonly ILogger _logger = null!;
    
    private Font _font = null!;
    
    public void OnPostInject()
    {
        _font = _resource.Load<Font>("/fonts/OpenSans.ttf", [("size", 18)]);
    }

    public override void Draw(IRenderContext renderer, DrawPayload payload)
    {
        DrawCameraInfo(renderer, payload);
        DrawChunkGrid(renderer, payload.Camera);
    }
    
    private void DrawCameraInfo(IRenderContext renderer, DrawPayload payload)
    {
        var cameraStringBuilder = new StringBuilder();
        
        cameraStringBuilder.AppendLine("Camera params");
        cameraStringBuilder.AppendLine($" > Pos: {payload.Camera.Position}");
        cameraStringBuilder.AppendLine($" > Rot: {payload.Camera.Rotation}");
        cameraStringBuilder.AppendLine($" > Scl: {payload.Camera.Scale}");
        cameraStringBuilder.AppendLine($" > Siz: {payload.Camera.Size}");
        cameraStringBuilder.AppendLine("Input");
        cameraStringBuilder.AppendLine($" > Mouse Pos: {_inputHandler.MousePosition.ToString()}");
        cameraStringBuilder.AppendLine("FPS");
        cameraStringBuilder.AppendLine($" > Val: {_render.FrameCounter.Fps.ToString("0.###")}");
        cameraStringBuilder.AppendLine($" > Avg: {_render.FrameCounter.AvgFps.ToString("0.###")}");
        cameraStringBuilder.AppendLine($" > Min: {_render.FrameCounter.MinFps.ToString("0.###")}");
        cameraStringBuilder.AppendLine($" > Max: {_render.FrameCounter.MaxFps.ToString("0.###")}");
        cameraStringBuilder.AppendLine("Graphics");
        cameraStringBuilder.AppendLine($" > Delta: {_render.FrameCounter.DeltaTime.ToString("0.###")}");
        cameraStringBuilder.AppendLine($" > FrmTm: {_render.FrameCounter.FrameTimeMs.ToString("0.###")}");
        cameraStringBuilder.AppendLine($" > Batching: {_render.BatchCount}");
        cameraStringBuilder.AppendLine($" > Vertices: {_render.VerticesCount}");

        using (renderer.UseRenderState(payload.Window))
        {
            renderer.DrawText(cameraStringBuilder.ToString(), _font, new Vector2(0, -16), Color.White);
        }
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
                renderer.DrawText(chunkId, _font, center, gridColor);
            }
        }
    }
}