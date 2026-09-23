using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda4.Definitions;
using Zelda4.Entities;
using Zelda4.States.PlayerStates;
using Zelda4.World;

namespace Zelda4.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private Player _player;
    private Room _room;

    private Tileset _tileset;
    private TextureAtlas _heartsAtlas;

    public override void Enter()
    {
        var tilesheet = Game.Content.Load<Texture2D>("images/tilesheet");
        _tileset     = new Tileset(new TextureRegion(tilesheet, 0, 0, tilesheet.Width, tilesheet.Height), GameSettings.TileSize, GameSettings.TileSize);
        _heartsAtlas = TextureAtlas.FromGrid(Game.Content.Load<Texture2D>("images/hearts"), GameSettings.TileSize, GameSettings.TileSize);

        _player = new Player
        {
            Position  = new Vector2(
                GameSettings.VirtualWidth  / 2f - GameSettings.PlayerWidth  / 2f,
                GameSettings.VirtualHeight / 2f - GameSettings.PlayerHeight / 2f),
            Width     = GameSettings.PlayerWidth,
            Height    = GameSettings.PlayerHeight,
            WalkSpeed = GameSettings.PlayerWalkSpeed,
            Health    = GameSettings.PlayerStartHealth
        };

        foreach (var (key, anim) in EntityDefinitions.CreatePlayerAnimations())
            _player.Animations.Add(key, anim);

        _room = new Room(_player, _tileset);

        _room.OnPlayerDied += OnPlayerDied;

        _player.ChangeState(new PlayerIdleState(_player, _room));
    }

    private void OnPlayerDied()
    {
        Game.SetState(new GameOverState(Game));
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
        DrawHearts(spriteBatch);
        spriteBatch.End();
    }

    private void DrawHearts(SpriteBatch spriteBatch)
    {
        int healthLeft = _player.Health;
        int ts = GameSettings.TileSize;

        for (int i = 0; i < GameSettings.HeartCount; i++)
        {
            int frame = healthLeft > 1 ? GameSettings.HeartFrameFull
                      : healthLeft == 1 ? GameSettings.HeartFrameHalf
                      : GameSettings.HeartFrameEmpty;

            _heartsAtlas.GetRegion($"frame_{frame}")
                .Draw(spriteBatch, new Vector2(i * (ts + GameSettings.HeartHudGap), GameSettings.HeartHudOffsetY), Color.White);

            healthLeft -= GameSettings.HeartHealthPerHeart;
        }
    }
}
