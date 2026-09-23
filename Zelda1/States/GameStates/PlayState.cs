using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda1.Definitions;
using Zelda1.Entities;
using Zelda1.States.PlayerStates;
using Zelda1.World;

namespace Zelda1.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private Player _player;
    private Room _room;

    private Tileset _tileset;

    public override void Enter()
    {
        var tilesheet = Game.Content.Load<Texture2D>("images/tilesheet");
        _tileset     = new Tileset(new TextureRegion(tilesheet, 0, 0, tilesheet.Width, tilesheet.Height), GameSettings.TileSize, GameSettings.TileSize);

        _player = new Player
        {
            Position  = new Vector2(
                GameSettings.VirtualWidth  / 2f - GameSettings.PlayerWidth  / 2f,
                GameSettings.VirtualHeight / 2f - GameSettings.PlayerHeight / 2f),
            Width     = GameSettings.PlayerWidth,
            Height    = GameSettings.PlayerHeight,
            WalkSpeed = GameSettings.PlayerWalkSpeed,
        };

        foreach (var (key, anim) in EntityDefinitions.CreatePlayerAnimations())
            _player.Animations.Add(key, anim);

        _room = new Room(_player, _tileset);

        _player.ChangeState(new PlayerIdleState(_player, _room));
    }

    public override void Update(GameTime gameTime)
    {
        _room.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        _room.Render(spriteBatch);
        _player.Draw(spriteBatch);
        spriteBatch.End();
    }
}
