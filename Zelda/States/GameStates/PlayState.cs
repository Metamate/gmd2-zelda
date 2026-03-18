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
        _pixel.SetData(new[] { Color.White });

        // Load all atlases by splitting textures into uniform grids
        var tilesTex     = Game.Content.Load<Texture2D>("images/tilesheet");
        var walkTex      = Game.Content.Load<Texture2D>("images/character_walk");
        var swordTex     = Game.Content.Load<Texture2D>("images/character_swing_sword");
        var entityTex    = Game.Content.Load<Texture2D>("images/entities");
        var heartsTex    = Game.Content.Load<Texture2D>("images/hearts");
        var switchesTex  = Game.Content.Load<Texture2D>("images/switches");

        _tileset      = new Tileset(new TextureRegion(tilesTex, 0, 0, tilesTex.Width, tilesTex.Height), 16, 16);
        _entityAtlas  = TextureAtlas.FromGrid(entityTex,   16, 16);
        _switchAtlas  = TextureAtlas.FromGrid(switchesTex, 16, 18);
        _heartsAtlas  = TextureAtlas.FromGrid(heartsTex,   16, 16);

        var walkAtlas  = TextureAtlas.FromGrid(walkTex,  16, 32);
        var swordAtlas = TextureAtlas.FromGrid(swordTex, 32, 32);

        _player = new Player
        {
            Position  = new Vector2(
                GameSettings.VirtualWidth  / 2f - 8,
                GameSettings.VirtualHeight / 2f - 11),
            Width     = 16,
            Height    = 22,
            WalkSpeed = GameSettings.PlayerWalkSpeed,
            Health    = 6   // three hearts × 2 health per heart
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

        for (int i = 0; i < 3; i++)
        {
            // Frame 5 = full heart, frame 3 = half heart, frame 1 = empty heart
            int frame;
            if (healthLeft > 1)
                frame = 5;
            else if (healthLeft == 1)
                frame = 3;
            else
                frame = 1;

            _heartsAtlas.GetRegion($"frame_{frame}")
                .Draw(spriteBatch, new Vector2(i * (ts + 1), 2), Color.White);

            healthLeft -= 2;
        }
    }
}
