using Microsoft.Xna.Framework;

namespace Zelda.Graphics;

public class Camera
{
    public Matrix Transform { get; private set; } = Matrix.Identity;
    public Vector2 Position { get; set; }

    public void Follow(Vector2 target, int viewportWidth, int viewportHeight)
    {
        Position = target;

        var center = new Vector2(viewportWidth / 2f, viewportHeight / 2f);
        Transform = Matrix.CreateTranslation(new Vector3(-Position, 0)) * // Shifts the world based on target position
                    Matrix.CreateTranslation(new Vector3(center, 0)); // Centers the target in the viewport
    }
}
