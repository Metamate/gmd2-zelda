using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Input;

namespace Zelda.States.GameStates;

public class GameOverState(Game1 game) : GameStateBase(game)
{
    private const string Title    = "GAME OVER";
    private const string Subtitle = "Press Enter";

    private Vector2 _titlePos;
    private Vector2 _subtitlePos;

    public override void Enter()
    {
        (_titlePos, _subtitlePos) = CalculateTitleLayout(Title, Subtitle);
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Confirm)
            Game.SetState(new StartState(Game));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.DrawString(Game1.DefaultFont, Title,    _titlePos,    GameSettings.GameOverColor);
        spriteBatch.DrawString(Game1.DefaultFont, Subtitle, _subtitlePos, Color.White,
            0f, Vector2.Zero, GameSettings.UiSubtitleScale, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
