using GMDCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Zelda.Input;
using Zelda.LevelMaker;

namespace Zelda.States.GameStates;

public class StartState(Game1 game) : GameStateBase(game)
{
    private string _title = "The Legend of VIA";
    private Vector2 _titlePosition;
    private string _subtitle = "Press Enter";
    private Vector2 _subtitlePosition;
    private float _subtitleScale = 0.5f;
    private GameLevel _backgroundLevel;

    public override void Enter()
    {
        Vector2 size = Game1.DefaultFont.MeasureString(_title);
        _titlePosition = new Vector2(
            GameSettings.VirtualWidth / 2f - size.X / 2f,
            GameSettings.VirtualHeight / 2f - size.Y / 2f - 10f
        );

        Vector2 subtitleSize = Game1.DefaultFont.MeasureString(_subtitle) * _subtitleScale;
        _subtitlePosition = new Vector2(
            GameSettings.VirtualWidth / 2f - subtitleSize.X / 2f,
            _titlePosition.Y + size.Y + 5f
        );

        var levelMaker = new ComplexLevelMaker(Game.Content);
        _backgroundLevel = levelMaker.Generate(30, 9);
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Start)
        {
            Game.SetState(new PlayState(Game));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _backgroundLevel.Draw(spriteBatch, Game.ScreenScaleMatrix);

        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.DrawString(Game1.DefaultFont, _title, _titlePosition, Color.White);
        spriteBatch.DrawString(Game1.DefaultFont, _subtitle, _subtitlePosition, Color.White, 0f, Vector2.Zero, _subtitleScale, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
