using Microsoft.Xna.Framework;

namespace Zelda.Graphics;

// Simple 2-D camera: stores a world-space position and exposes a translation
// matrix suitable for combining with SpriteBatch's transformMatrix parameter.
public class Camera
{
    public Vector2 Position { get; set; }

    // Translates draw coordinates so the point at Position appears at the
    // origin of the viewport.  Combine with the screen-scale matrix:
    //   camera.Transform * screenScaleMatrix
    public Matrix Transform => Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
}
