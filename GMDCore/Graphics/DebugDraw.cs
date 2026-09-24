using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

// Draws outlines, for seeing what the game can't show: hitboxes, solid tiles, trigger areas.
// Nothing is drawn unless Enabled is true, so the calls can stay in the code.
public static class DebugDraw
{
    private static Texture2D _pixel;

    public static bool Enabled { get; set; }

    public static void Rectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        if (!Enabled)
        {
            return;
        }

        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData([Color.White]);
        }

        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Right - 1, rectangle.Top, 1, rectangle.Height), color);
    }
}
