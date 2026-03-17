using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Input;
using Zelda.Audio;
using Zelda.Entities;

namespace Zelda.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private Player _player;

    public override void Enter()
    {
        TextureAtlas alienAtlas = TextureAtlas.FromFile(Game.Content, "images/hero.xml");
        _player = new Player(alienAtlas);

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
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        spriteBatch.DrawString(Game1.DefaultFont, $"Score: {_player.Score}", new Vector2(5, 5), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
