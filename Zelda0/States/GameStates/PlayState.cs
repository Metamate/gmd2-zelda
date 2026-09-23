using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda0.Input;
using Zelda0.World;

namespace Zelda0.States.GameStates;

public class PlayState(Game1 game) : GameStateBase(game)
{
    private Tileset _tileset;
    private Room _room;

    public override void Enter()
    {
        var tilesheet = Game.Content.Load<Texture2D>("images/tilesheet");
        _tileset = new Tileset(new TextureRegion(tilesheet, 0, 0, tilesheet.Width, tilesheet.Height), GameSettings.TileSize, GameSettings.TileSize);
        _room = new Room(_tileset);
    }

    public override void Update(GameTime gameTime)
    {
        // Press Enter to generate a new room.
        if (GameController.Confirm)
            _room = new Room(_tileset);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(transformMatrix: Game.ScreenScaleMatrix, samplerState: SamplerState.PointClamp);
        _room.Render(spriteBatch);
        spriteBatch.End();
    }
}
