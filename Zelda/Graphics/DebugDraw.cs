using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.Graphics;

// Minimal debug utility for drawing filled rectangles over the game world.
// The pixel texture is created once on first use and reused every frame.
public static class DebugDraw
{
    private static Texture2D _pixel;

    public static void FillRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        _pixel ??= CreatePixel(spriteBatch.GraphicsDevice);
        spriteBatch.Draw(_pixel, rect, color);
    }

    private static Texture2D CreatePixel(GraphicsDevice graphicsDevice)
    {
        var tex = new Texture2D(graphicsDevice, 1, 1);
        tex.SetData([Color.White]);
        return tex;
    }
}
