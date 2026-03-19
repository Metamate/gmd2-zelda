using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Audio;
using Zelda.Definitions;
using Zelda.Entities;
using Zelda.States.PlayerStates;
using Zelda.World;

namespace Zelda.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private Player _player;
    private Dungeon _dungeon;

    private Tileset _tileset;
    private TextureAtlas _entityAtlas;
    private TextureAtlas _switchAtlas;
    private TextureAtlas _heartsAtlas;

    // 1×1 white pixel used as a solid fill when writing the stencil arch mask.
    private Texture2D _pixel;

    public override void Enter()
    {
        // 1×1 white pixel for stencil mask rendering
        _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        var tilesTex    = Game.Content.Load<Texture2D>("images/tilesheet");
        var walkTex     = Game.Content.Load<Texture2D>("images/character_walk");
        var swordTex    = Game.Content.Load<Texture2D>("images/character_swing_sword");
        var entityTex   = Game.Content.Load<Texture2D>("images/entities");
        var heartsTex   = Game.Content.Load<Texture2D>("images/hearts");
        var switchesTex = Game.Content.Load<Texture2D>("images/switches");

        _tileset     = new Tileset(new TextureRegion(tilesTex, 0, 0, tilesTex.Width, tilesTex.Height), 16, 16);
        _entityAtlas = TextureAtlas.FromGrid(entityTex,   16, 16);
        _heartsAtlas = TextureAtlas.FromGrid(heartsTex,   16, 16);
        _switchAtlas = TextureAtlas.FromGrid(switchesTex, 16, 18);

        var walkAtlas  = TextureAtlas.FromGrid(walkTex,  16, 32);
        var swordAtlas = TextureAtlas.FromGrid(swordTex, 32, 32);

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

        foreach (var (key, anim) in EntityDefinitions.CreatePlayerAnimations(walkAtlas, swordAtlas))
            _player.Animations.Add(key, anim);

        _dungeon = new Dungeon(_player, () => new Room(
            _player, _tileset, _entityAtlas, _switchAtlas));

        _dungeon.OnPlayerDied += OnPlayerDied;

        _player.ChangeState(new PlayerIdleState(_player, _dungeon));

        SoundManager.PlayMusic();
    }

    public override void Exit()
    {
        SoundManager.StopMusic();
        _pixel?.Dispose();
        _pixel = null;
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
        // Dungeon handles its own Begin/End passes (3 passes for stencil arch clipping).
        _dungeon.Render(spriteBatch, Game.ScreenScaleMatrix, _pixel);

        // HUD drawn in a clean pass after dungeon, with no stencil state active.
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
                .Draw(spriteBatch, new Vector2(i * (ts + 1), 2), Color.White);

            healthLeft -= GameSettings.HeartHealthPerHeart;
        }
    }
}
