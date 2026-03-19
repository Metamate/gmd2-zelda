using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Input;

namespace Zelda.States.GameStates;

public class StartState(Game1 game) : GameStateBase(game)
{
    private const string Title    = "The Legend of VIA";
    private const string Subtitle = "Press Enter";

    private Vector2 _titlePos;
    private Vector2 _subtitlePos;

    public override void Enter()
    {
        var titleSize = Game1.DefaultFont.MeasureString(Title);
        _titlePos = ScreenCenter(Title, GameSettings.UiTitleYOffset);

        // Subtitle sits below the title; X is independently centred at subtitle scale
        _subtitlePos = new Vector2(
            ScreenCenter(Subtitle, scale: GameSettings.UiSubtitleScale).X,
            _titlePos.Y + titleSize.Y + GameSettings.UiSubtitleSpacing);
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Confirm)
            Game.SetState(new PlayState(Game));
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.DrawString(Game1.DefaultFont, Title,    _titlePos,    Color.White);
        spriteBatch.DrawString(Game1.DefaultFont, Subtitle, _subtitlePos, Color.White,
            0f, Vector2.Zero, GameSettings.UiSubtitleScale, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
