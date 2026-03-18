using System;
using System.Collections.Generic;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Audio;
using Zelda.Definitions;
using Zelda.Entities;
using Zelda.States.EntityStates;

namespace Zelda.World;

public class Room
{
    private readonly Tilemap _tilemap;
    private readonly TextureAtlas _entityAtlas;
    private readonly TextureAtlas _switchAtlas;

    private readonly Player _player;

    public List<Enemy> Enemies { get; } = new();
    public List<GameObject> Objects { get; } = new();
    public List<Doorway> Doorways { get; } = new();

    // Shared RNG exposed so entity states can use it
    public Random Random { get; } = new();

    private readonly int _renderOffsetX = GameSettings.MapRenderOffsetX;
    private readonly int _renderOffsetY = GameSettings.MapRenderOffsetY;

    private static readonly string[] EnemyTypes = ["skeleton", "slime", "bat", "ghost", "spider"];

    public Room(
        Player player,
        Tileset tileset,
        TextureAtlas entityAtlas,
        TextureAtlas switchAtlas)
    {
        _player      = player;
        _entityAtlas = entityAtlas;
        _switchAtlas = switchAtlas;

        _tilemap = new Tilemap(tileset, GameSettings.MapWidth, GameSettings.MapHeight);
        GenerateWallsAndFloors();
        GenerateEntities();
        GenerateObjects();
        GenerateDoorways();
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

                if      (x == 0     && y == 0    ) tileId = GameSettings.TileTopLeftCorner;
                else if (x == w - 1 && y == 0    ) tileId = GameSettings.TileTopRightCorner;
                else if (x == 0     && y == h - 1) tileId = GameSettings.TileBottomLeftCorner;
                else if (x == w - 1 && y == h - 1) tileId = GameSettings.TileBottomRightCorner;
                else if (x == 0    ) tileId = GameSettings.TileLeftWalls[Random.Next(GameSettings.TileLeftWalls.Length)];
                else if (x == w - 1) tileId = GameSettings.TileRightWalls[Random.Next(GameSettings.TileRightWalls.Length)];
                else if (y == 0    ) tileId = GameSettings.TileTopWalls[Random.Next(GameSettings.TileTopWalls.Length)];
                else if (y == h - 1) tileId = GameSettings.TileBottomWalls[Random.Next(GameSettings.TileBottomWalls.Length)];
                else                 tileId = GameSettings.TileFloors[Random.Next(GameSettings.TileFloors.Length)];

                // Tile IDs in GameSettings are 1-based (Löve2D compat); Tileset is 0-based.
                _tilemap.SetTile(x, y, new Tile(tileId - 1, solid));
            }
        }
    }

    private void GenerateEntities()
    {
        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapH = GameSettings.MapHeight;

        int minX = offX + ts;
        int maxX = GameSettings.VirtualWidth - ts * 2 - 16;
        int minY = offY + ts;
        int maxY = offY + mapH * ts - ts - 16;

        for (int i = 0; i < 10; i++)
        {
            string type = EnemyTypes[Random.Next(EnemyTypes.Length)];

            var enemy = new Enemy
            {
                Position  = new Vector2(Random.Next(minX, maxX + 1), Random.Next(minY, maxY + 1)),
                Width     = 16,
                Height    = 16,
                WalkSpeed = 20,
                Health    = 1
            };

            foreach (var (key, anim) in EntityDefinitions.CreateEnemyAnimations(type, _entityAtlas))
                enemy.Animations.Add(key, anim);

            enemy.ChangeState(new EntityWalkState(enemy));
            Enemies.Add(enemy);
        }
    }

    private void GenerateObjects()
    {
        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapH = GameSettings.MapHeight;

        int minX = offX + ts;
        int maxX = GameSettings.VirtualWidth - ts * 2 - 16;
        int minY = offY + ts;
        int maxY = offY + mapH * ts - ts - 16;

        var switchObj = new GameObject(
            type: "switch",
            atlas: _switchAtlas,
            stateFrames: new() { ["unpressed"] = 2, ["pressed"] = 1 },
            defaultState: "unpressed",
            width: 16,
            height: 16
        );
        switchObj.Position = new Vector2(Random.Next(minX, maxX + 1), Random.Next(minY, maxY + 1));

        switchObj.OnCollide = () =>
        {
            if (switchObj.State == "unpressed")
            {
                switchObj.State = "pressed";
                foreach (var d in Doorways)
                    d.IsOpen = true;
                SoundManager.PlaySound("door");
            }
        };

        Objects.Add(switchObj);
    }

    private void GenerateDoorways()
    {
        Doorways.Add(new Doorway(Direction.Up,    false, _tilemap.Tileset));
        Doorways.Add(new Doorway(Direction.Down,  false, _tilemap.Tileset));
        Doorways.Add(new Doorway(Direction.Left,  false, _tilemap.Tileset));
        Doorways.Add(new Doorway(Direction.Right, false, _tilemap.Tileset));
    }

    public void Update(GameTime gameTime)
    {
        _player.Update(gameTime);

        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            var enemy = Enemies[i];

            if (enemy.Health <= 0)
            {
                enemy.Active = false;
                Enemies.RemoveAt(i);
                continue;
            }

            enemy.ProcessAI(this, gameTime);
            enemy.Update(gameTime);

            // Player–enemy collision
            if (_player.Collides(enemy) && !_player.IsInvulnerable)
            {
                SoundManager.PlaySound("hit-player");
                _player.Damage(1);
                _player.GoInvulnerable(1.5f);

                if (_player.Health <= 0)
                    OnPlayerDied?.Invoke();
            }
        }

        foreach (var obj in Objects)
        {
            obj.Update(gameTime);

            if (_player.Collides(obj))
                obj.OnCollide();
        }
    }

    // Raised when the player's health reaches zero; PlayState subscribes to this.
    public event Action OnPlayerDied;

    public void Render(SpriteBatch spriteBatch, Vector2 adjacentOffset)
    {
        int ts   = GameSettings.TileSize;
        int offX = _renderOffsetX;
        int offY = _renderOffsetY;
        int w    = GameSettings.MapWidth;
        int h    = GameSettings.MapHeight;

        // Draw tiles via Tileset, offsetting into world/room position
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var tile = _tilemap.GetTile(x, y);
                var pos  = new Vector2(x * ts + offX + adjacentOffset.X, y * ts + offY + adjacentOffset.Y);
                _tilemap.Tileset.GetTile(tile.GraphicId).Draw(spriteBatch, pos, Color.White);
            }
        }

        // Draw doorways
        foreach (var doorway in Doorways)
            doorway.DrawAt(spriteBatch, adjacentOffset);

        // Draw objects (switches, etc.)
        foreach (var obj in Objects)
        {
            var savedPos = obj.Position;
            obj.Position += adjacentOffset;
            obj.Draw(spriteBatch);
            obj.Position = savedPos;
        }

        // Draw enemies
        foreach (var enemy in Enemies)
        {
            var savedPos = enemy.Position;
            enemy.Position += adjacentOffset;
            enemy.Draw(spriteBatch);
            enemy.Position = savedPos;
        }
    }
}
