using System;
using System.Collections.Generic;
using System.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda2.Definitions;
using Zelda2.Entities;
using Zelda2.States.EntityStates;

namespace Zelda2.World;

public class Room
{
    private readonly Tilemap _tilemap;
    private readonly Player _player;

    public List<Enemy> Enemies { get; } = [];

    // Shared RNG exposed so entity states can use it
    public Random Random { get; } = new();

    public Room(Player player, Tileset tileset)
    {
        _player = player;
        _tilemap = new Tilemap(tileset, GameSettings.MapWidth, GameSettings.MapHeight)
        {
            Position = new Vector2(GameSettings.MapRenderOffsetX, GameSettings.MapRenderOffsetY)
        };
        GenerateWallsAndFloors();
        GenerateEntities();
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

    private (int MinX, int MaxX, int MinY, int MaxY) GetSpawnBounds()
    {
        int ts = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapH = GameSettings.MapHeight;

        return (
            MinX: offX + ts,
            MaxX: GameSettings.VirtualWidth - ts * 2 - ts,
            MinY: offY + ts,
            MaxY: offY + mapH * ts - ts * 2
        );
    }

    private void GenerateEntities()
    {
        var (minX, maxX, minY, maxY) = GetSpawnBounds();
        var enemyTypes = EntityDefinitions.EnemyTypes.ToArray();

        for (int i = 0; i < GameSettings.RoomEnemyCount; i++)
        {
            string type = enemyTypes[Random.Next(enemyTypes.Length)];
            var stats = EntityDefinitions.GetEnemyStats(type);

            var enemy = new Enemy
            {
                Position = new Vector2(Random.Next(minX, maxX + 1), Random.Next(minY, maxY + 1)),
                Width = stats.Width,
                Height = stats.Height,
                WalkSpeed = stats.WalkSpeed
            };

            foreach (var (key, anim) in EntityDefinitions.CreateEnemyAnimations(type))
                enemy.Animations.Add(key, anim);

            enemy.ChangeState(new EntityWalkState(enemy));
            Enemies.Add(enemy);
        }
    }

    public void Update(GameTime gameTime)
    {
        _player.Update(gameTime);

        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            var enemy = Enemies[i];

            enemy.ProcessAI(this, gameTime);
            enemy.Update(gameTime);
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        _tilemap.Draw(spriteBatch);

        foreach (var enemy in Enemies)
            enemy.Draw(spriteBatch);
    }
}
