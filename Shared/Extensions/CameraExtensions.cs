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
}