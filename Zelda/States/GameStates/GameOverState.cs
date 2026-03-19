using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Input;

namespace Zelda.States.GameStates;

public class GameOverState(Game1 game) : GameStateBase(game)
{
    private const string Title    = "GAME OVER";
    private const string Subtitle = "Press Enter";

    public override void Update(GameTime gameTime)
    {
        if (GameController.Confirm)
            Game.SetState(new StartState(Game));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var font      = Game1.DefaultFont;
        var titleSize = font.MeasureString(Title);
        var titlePos  = ScreenCenter(Title, GameSettings.UiTitleYOffset);
        var subPos    = new Vector2(
            ScreenCenter(Subtitle, scale: GameSettings.UiSubtitleScale).X,
            titlePos.Y + titleSize.Y + GameSettings.UiSubtitleSpacing);

        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.DrawString(font, Title,    titlePos, GameSettings.GameOverColor);
        spriteBatch.DrawString(font, Subtitle, subPos,   Color.White,
            0f, Vector2.Zero, GameSettings.UiSubtitleScale, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
