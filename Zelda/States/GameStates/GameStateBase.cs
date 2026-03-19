using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.States.GameStates;

public abstract class GameStateBase(Game1 game)
{
    protected Game1 Game { get; } = game;

    public virtual void Enter() { }
    public virtual void Exit() { }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);

    // Returns the position that centres text horizontally and vertically,
    // with an optional Y offset from dead centre.  Scale applies to the
    // measured size so scaled DrawString calls position correctly.
    protected static Vector2 ScreenCenter(string text, float yOffset = 0f, float scale = 1f)
    {
        Vector2 size = Game1.DefaultFont.MeasureString(text) * scale;
        return new Vector2(
            GameSettings.VirtualWidth  / 2f - size.X / 2f,
            GameSettings.VirtualHeight / 2f - size.Y / 2f + yOffset);
    }
}
