using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Input;

namespace Zelda.States.GameStates;

public class GameOverState(Game1 game) : GameStateBase(game)
{
    public override void Update(GameTime gameTime)
    {
        if (GameController.Confirm)
            Game.SetState(new StartState(Game));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);

        var font = Game1.DefaultFont;

        string gameOver  = "GAME OVER";
        string pressEnter = "Press Enter";

        Vector2 goSize = font.MeasureString(gameOver);
        Vector2 goPos = new(
            GameSettings.VirtualWidth / 2f  - goSize.X / 2f,
            GameSettings.VirtualHeight / 2f - goSize.Y / 2f - 10f);

        spriteBatch.DrawString(font, gameOver, goPos, new Color(175, 53, 42));

        Vector2 subSize = font.MeasureString(pressEnter) * 0.5f;
        Vector2 subPos  = new(
            GameSettings.VirtualWidth / 2f  - subSize.X / 2f,
            goPos.Y + goSize.Y + 5f);

        spriteBatch.DrawString(font, pressEnter, subPos, Color.White, 0f, Vector2.Zero, 0.5f,
            SpriteEffects.None, 0f);

        spriteBatch.End();
    }
}
