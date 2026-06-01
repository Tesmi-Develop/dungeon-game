using Hypercube.Core.Viewports;
using Hypercube.Mathematics.Vectors;

namespace Shared.Extensions;

public static class CameraExtensions
{
    public static Vector3 ScreenToWorld(this ICamera camera, Vector2 mousePosition)
    {
        var size = camera.Size;
        
        if (size.X <= 0 || size.Y <= 0)
            return Vector3.Zero;
        
        var ndcX = (2.0f * mousePosition.X / size.X) - 1.0f;
        var ndcY = 1.0f - (2.0f * mousePosition.Y / size.Y);
        
        var ndcPosition = new Vector4(ndcX, ndcY, 0.0f, 1.0f);
        
        var viewProjection = camera.View * camera.Projection;
        var invertedVp = viewProjection.Inverted();
        
        var worldPositionWithW = ndcPosition * invertedVp;
        
        return new Vector3(
            worldPositionWithW.X / worldPositionWithW.W,
            worldPositionWithW.Y / worldPositionWithW.W,
            worldPositionWithW.Z / worldPositionWithW.W
        );
    }

    public static Vector2 WorldToScreen(this ICamera camera, Vector2 worldPosition)
    {
        return camera.WorldToScreen((Vector3)worldPosition);
    }
    
    public static Vector2 WorldToScreen(this ICamera camera, Vector3 worldPosition)
    {
        var size = camera.Size;
    
        if (size.X <= 0 || size.Y <= 0)
            return Vector2.Zero;
        
        var viewProjection = camera.View * camera.Projection;
        var worldPos4 = new Vector4(worldPosition, 1.0f);
        var clipSpacePos = worldPos4 * viewProjection;
        
        if (clipSpacePos.W == 0) 
            return Vector2.Zero;
    
        var ndc = new Vector3(
            clipSpacePos.X / clipSpacePos.W,
            clipSpacePos.Y / clipSpacePos.W,
            clipSpacePos.Z / clipSpacePos.W
        );
        
        var screenX = (ndc.X + 1.0f) * 0.5f * size.X;
        var screenY = (1.0f + ndc.Y) * 0.5f * size.Y;

        return new Vector2(screenX, screenY);
    }
}