using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda5.Definitions;
using Zelda5.Entities;
using Zelda5.States.PlayerStates;
using Zelda5.World;

namespace Zelda5.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private Player _player;
    private Dungeon _dungeon;

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

        _dungeon = new Dungeon(_player, () => new Room(_player, _tileset));

        _dungeon.OnPlayerDied += OnPlayerDied;

        _player.ChangeState(new PlayerIdleState(_player, _dungeon));
    }

    private void OnPlayerDied()
    {
        Game.SetState(new GameOverState(Game));
    }

    public override void Update(GameTime gameTime)
    {
        _dungeon.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Dungeon handles its own Begin/End passes.
        _dungeon.Render(spriteBatch, Game.ScreenScaleMatrix);

        // HUD drawn in its own pass after the dungeon, without the camera.
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
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
