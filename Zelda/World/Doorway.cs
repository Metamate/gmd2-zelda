using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

    // Each door variant is 4 tiles; each entry is (tile ID, x offset, y offset).
    private readonly record struct TilePlacement(int Id, int Dx, int Dy);

    private static Dictionary<Direction, TilePlacement[]> _openLayouts;
    private static Dictionary<Direction, TilePlacement[]> _closedLayouts;

    public static void LoadContent(ContentManager content)
    {
        string path = Path.Combine(content.RootDirectory, "data/door_layouts.xml");
        using Stream stream = TitleContainer.OpenStream(path);
        XDocument doc = XDocument.Load(stream);

        _openLayouts   = ParseLayouts(doc, "open");
        _closedLayouts = ParseLayouts(doc, "closed");
    }

    private static Dictionary<Direction, TilePlacement[]> ParseLayouts(XDocument doc, string state)
    {
        int ts = GameSettings.TileSize;
        return doc.Root
            .Elements("Layout")
            .Where(e => e.Attribute("state")?.Value == state)
            .ToDictionary(
                e => Enum.Parse<Direction>(e.Attribute("direction").Value, ignoreCase: true),
                e => e.Elements("Tile").Select(t => new TilePlacement(
                    int.Parse(t.Attribute("id").Value),
                    int.Parse(t.Attribute("dx").Value) * ts,
                    int.Parse(t.Attribute("dy").Value) * ts
                )).ToArray()
            );
    }

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
        foreach (var t in (IsOpen ? _openLayouts : _closedLayouts)[_direction])
            DrawTile(spriteBatch, t.Id, new Vector2(p.X + t.Dx, p.Y + t.Dy));
    }

    private void DrawTile(SpriteBatch spriteBatch, int tileId, Vector2 position)
    {
        _tileset.GetTile(tileId).Draw(spriteBatch, position, Color.White);
    }

    public bool Collides(IEntity other) =>
        Collidable && other.Collidable && Bounds.Intersects(other.Bounds);
}
