using System.Collections.Generic;
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
    private readonly Tileset _tileset;

    public bool IsOpen { get; set; }

    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);
    public bool Collidable { get; set; } = true;
    public bool IsSolid => false;
    public bool Active { get; set; } = true;

    // Each door variant is 4 tiles; each entry is (1-based tile ID, x offset, y offset).
    private readonly record struct TilePlacement(int Id, int Dx, int Dy);

    private const int Ts = GameSettings.TileSize;

    private static readonly Dictionary<Direction, TilePlacement[]> OpenLayouts = new()
    {
        [Direction.Left]  = [new(181, -Ts,  0), new(182,  0,  0), new(200, -Ts, Ts), new(201,  0, Ts)],
        [Direction.Right] = [new(172,   0,  0), new(173, Ts,  0), new(191,   0, Ts), new(192, Ts, Ts)],
        [Direction.Up]    = [new( 98,   0, -Ts), new( 99, Ts, -Ts), new(117,  0,  0), new(118, Ts,  0)],
        [Direction.Down]  = [new(141,   0,  0), new(142, Ts,  0), new(160,   0, Ts), new(161, Ts, Ts)],
    };

    private static readonly Dictionary<Direction, TilePlacement[]> ClosedLayouts = new()
    {
        [Direction.Left]  = [new(219, -Ts,  0), new(220,  0,  0), new(238, -Ts, Ts), new(239,  0, Ts)],
        [Direction.Right] = [new(174,   0,  0), new(175, Ts,  0), new(193,   0, Ts), new(194, Ts, Ts)],
        [Direction.Up]    = [new(134,   0, -Ts), new(135, Ts, -Ts), new(153,  0,  0), new(154, Ts,  0)],
        [Direction.Down]  = [new(216,   0,  0), new(217, Ts,  0), new(235,   0, Ts), new(236, Ts, Ts)],
    };

    public Doorway(Direction direction, bool open, Tileset tileset)
    {
        _direction = direction;
        IsOpen = open;
        _tileset = tileset;

        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapW = GameSettings.MapWidth;
        int mapH = GameSettings.MapHeight;

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
        Vector2 p = Position + adjacentOffset;
        foreach (var t in (IsOpen ? OpenLayouts : ClosedLayouts)[_direction])
            DrawTile(spriteBatch, t.Id, new Vector2(p.X + t.Dx, p.Y + t.Dy));
    }

    private void DrawTile(SpriteBatch spriteBatch, int tileId, Vector2 position)
    {
        // tileId is 1-based (Löve2D layout); Tileset is 0-based
        _tileset.GetTile(tileId - 1).Draw(spriteBatch, position, Color.White);
    }

    public bool Collides(IEntity other) =>
        Collidable && other.Collidable && Bounds.Intersects(other.Bounds);
}
