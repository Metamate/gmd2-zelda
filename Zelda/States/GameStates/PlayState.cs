using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Input;
using Zelda.LevelMaker;
using Zelda.Audio;
using Zelda.Entities;

namespace Zelda.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private LevelMakerBase _levelMaker;
    private Player _player;
    private GameLevel _currentLevel;

    public override void Enter()
    {
        //TextureAtlas alienAtlas = TextureAtlas.FromFile(Game.Content, "images/hero.xml");
        //_player = new Player(alienAtlas, _currentLevel);
        //_currentLevel.Player = _player;

        SoundManager.PlayMusic();
    }

    public override void Exit()
    {
        SoundManager.StopMusic();
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Reset)
        {
            Game.SetState(new StartState(Game));
        }

        _currentLevel.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _currentLevel.Draw(spriteBatch, Game.ScreenScaleMatrix);

        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.DrawString(Game1.DefaultFont, $"Score: {_player.Score}", new Vector2(5, 5), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
