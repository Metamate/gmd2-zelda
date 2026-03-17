using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Entities;

namespace Zelda.World;

// A doorway opening in one of the four walls of a Room.
// The door renders as a 2x2 tile composite; the Bounds rectangle is used for
// player collision to trigger room transitions.
public class Doorway : IEntity
{
    private readonly Direction _direction;
    private readonly TextureAtlas _tileAtlas;

    public bool IsOpen { get; set; }

    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);
    public bool Collidable { get; set; } = true;
    public bool IsSolid => false;
    public bool Active { get; set; } = true;

    public Doorway(Direction direction, bool open, TextureAtlas tileAtlas)
    {
        _direction = direction;
        IsOpen = open;
        _tileAtlas = tileAtlas;

        int ts      = GameSettings.TileSize;
        int offX    = GameSettings.MapRenderOffsetX;
        int offY    = GameSettings.MapRenderOffsetY;
        int mapW    = GameSettings.MapWidth;
        int mapH    = GameSettings.MapHeight;

        switch (direction)
        {
            case Direction.Left:
                Position = new Vector2(offX, offY + mapH / 2f * ts - ts);
                Width  = ts;
                Height = ts * 2;
                break;
            case Direction.Right:
                Position = new Vector2(offX + mapW * ts - ts, offY + mapH / 2f * ts - ts);
                Width  = ts;
                Height = ts * 2;
                break;
            case Direction.Up:
                Position = new Vector2(offX + mapW / 2f * ts - ts, offY);
                Width  = ts * 2;
                Height = ts;
                break;
            case Direction.Down:
                Position = new Vector2(offX + mapW / 2f * ts - ts, offY + mapH * ts - ts);
                Width  = ts * 2;
                Height = ts;
                break;
        }
    }

    public void Update(GameTime gameTime) { }

    public void Draw(SpriteBatch spriteBatch) => DrawAt(spriteBatch, Vector2.Zero);

    // Draw with an additional offset used during room-shift camera translation
    public void DrawAt(SpriteBatch spriteBatch, Vector2 adjacentOffset)
    {
        int ts = GameSettings.TileSize;
        Vector2 p = Position + adjacentOffset;

        // Tile indices match the Löve2D quad layout (1-based frame names from FromGrid).
        switch (_direction)
        {
            case Direction.Left:
                if (IsOpen)
                {
                    DrawTile(spriteBatch, 181, new Vector2(p.X - ts, p.Y));
                    DrawTile(spriteBatch, 182, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 200, new Vector2(p.X - ts, p.Y + ts));
                    DrawTile(spriteBatch, 201, new Vector2(p.X,      p.Y + ts));
                }
                else
                {
                    DrawTile(spriteBatch, 219, new Vector2(p.X - ts, p.Y));
                    DrawTile(spriteBatch, 220, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 238, new Vector2(p.X - ts, p.Y + ts));
                    DrawTile(spriteBatch, 239, new Vector2(p.X,      p.Y + ts));
                }
                break;

            case Direction.Right:
                if (IsOpen)
                {
                    DrawTile(spriteBatch, 172, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 173, new Vector2(p.X + ts, p.Y));
                    DrawTile(spriteBatch, 191, new Vector2(p.X,      p.Y + ts));
                    DrawTile(spriteBatch, 192, new Vector2(p.X + ts, p.Y + ts));
                }
                else
                {
                    DrawTile(spriteBatch, 174, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 175, new Vector2(p.X + ts, p.Y));
                    DrawTile(spriteBatch, 193, new Vector2(p.X,      p.Y + ts));
                    DrawTile(spriteBatch, 194, new Vector2(p.X + ts, p.Y + ts));
                }
                break;

            case Direction.Up:
                if (IsOpen)
                {
                    DrawTile(spriteBatch, 98,  new Vector2(p.X,      p.Y - ts));
                    DrawTile(spriteBatch, 99,  new Vector2(p.X + ts, p.Y - ts));
                    DrawTile(spriteBatch, 117, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 118, new Vector2(p.X + ts, p.Y));
                }
                else
                {
                    DrawTile(spriteBatch, 134, new Vector2(p.X,      p.Y - ts));
                    DrawTile(spriteBatch, 135, new Vector2(p.X + ts, p.Y - ts));
                    DrawTile(spriteBatch, 153, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 154, new Vector2(p.X + ts, p.Y));
                }
                break;

            case Direction.Down:
                if (IsOpen)
                {
                    DrawTile(spriteBatch, 141, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 142, new Vector2(p.X + ts, p.Y));
                    DrawTile(spriteBatch, 160, new Vector2(p.X,      p.Y + ts));
                    DrawTile(spriteBatch, 161, new Vector2(p.X + ts, p.Y + ts));
                }
                else
                {
                    DrawTile(spriteBatch, 216, new Vector2(p.X,      p.Y));
                    DrawTile(spriteBatch, 217, new Vector2(p.X + ts, p.Y));
                    DrawTile(spriteBatch, 235, new Vector2(p.X,      p.Y + ts));
                    DrawTile(spriteBatch, 236, new Vector2(p.X + ts, p.Y + ts));
                }
                break;
        }
    }

    private void DrawTile(SpriteBatch spriteBatch, int frameIndex, Vector2 position)
    {
        _tileAtlas.GetRegion($"frame_{frameIndex}").Draw(spriteBatch, position, Color.White);
    }

    public bool Collides(IEntity other) =>
        Collidable && other.Collidable && Bounds.Intersects(other.Bounds);
}
