using System;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda1.Entities;

namespace Zelda1.World;

public class Room
{
    private readonly Tilemap _tilemap;
    private readonly Player _player;

    public Random Random { get; } = new();

    public Room(Player player, Tileset tileset)
    {
        _player = player;
        _tilemap = new Tilemap(tileset, GameSettings.MapWidth, GameSettings.MapHeight)
        {
            Position = new Vector2(GameSettings.MapRenderOffsetX, GameSettings.MapRenderOffsetY)
        };
        GenerateWallsAndFloors();
    }

    private void GenerateWallsAndFloors()
    {
        int w = GameSettings.MapWidth;
        int h = GameSettings.MapHeight;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool solid = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                int tileId;

                if (x == 0 && y == 0) tileId = GameSettings.TileTopLeftCorner;
                else if (x == w - 1 && y == 0) tileId = GameSettings.TileTopRightCorner;
                else if (x == 0 && y == h - 1) tileId = GameSettings.TileBottomLeftCorner;
                else if (x == w - 1 && y == h - 1) tileId = GameSettings.TileBottomRightCorner;
                else if (x == 0) tileId = GameSettings.TileLeftWalls[Random.Next(GameSettings.TileLeftWalls.Length)];
                else if (x == w - 1) tileId = GameSettings.TileRightWalls[Random.Next(GameSettings.TileRightWalls.Length)];
                else if (y == 0) tileId = GameSettings.TileTopWalls[Random.Next(GameSettings.TileTopWalls.Length)];
                else if (y == h - 1) tileId = GameSettings.TileBottomWalls[Random.Next(GameSettings.TileBottomWalls.Length)];
                else tileId = GameSettings.TileFloors[Random.Next(GameSettings.TileFloors.Length)];

                _tilemap.SetTile(x, y, new Tile(tileId, solid));
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        _player.Update(gameTime);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        _tilemap.Draw(spriteBatch);
    }
}
