using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Player _player;

    public List<Enemy> Enemies { get; } = new();
    public List<GameObject> Objects { get; } = new();
    public List<Doorway> Doorways { get; } = new();

    // Shared RNG exposed so entity states can use it
    public Random Random { get; } = new();


    public Room(Player player, Tileset tileset)
    {
        _player  = player;
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

                _tilemap.SetTile(x, y, new Tile(tileId, solid));
            }
        }
    }

    private (int MinX, int MaxX, int MinY, int MaxY) GetSpawnBounds()
    {
        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapH = GameSettings.MapHeight;

        return (
            MinX: offX + ts,
            MaxX: GameSettings.VirtualWidth - ts * 2 - ts,
            MinY: offY + ts,
            MaxY: offY + mapH * ts - ts - ts
        );
    }

    private void GenerateEntities()
    {
        var (minX, maxX, minY, maxY) = GetSpawnBounds();
        var enemyTypes = EntityDefinitions.EnemyTypes.ToArray();

        for (int i = 0; i < GameSettings.RoomEnemyCount; i++)
        {
            string type  = enemyTypes[Random.Next(enemyTypes.Length)];
            var    stats = EntityDefinitions.GetEnemyStats(type);

            var enemy = new Enemy
            {
                Position  = new Vector2(Random.Next(minX, maxX + 1), Random.Next(minY, maxY + 1)),
                Width     = stats.Width,
                Height    = stats.Height,
                WalkSpeed = stats.WalkSpeed,
                Health    = stats.Health
            };

            foreach (var (key, anim) in EntityDefinitions.CreateEnemyAnimations(type))
                enemy.Animations.Add(key, anim);

            enemy.ChangeState(new EntityWalkState(enemy));
            Enemies.Add(enemy);
        }
    }

    private void GenerateObjects()
    {
        var (minX, maxX, minY, maxY) = GetSpawnBounds();

        var stats = GameObjectDefinitions.GetStats("switch");
        var switchObj = new GameObject(
            type: "switch",
            atlas: stats.Atlas,
            stateFrames: stats.StateFrames,
            defaultState: stats.DefaultState,
            width: stats.Width,
            height: stats.Height
        );
        switchObj.Position = new Vector2(Random.Next(minX, maxX + 1), Random.Next(minY, maxY + 1));

        // Behaviour is wired here rather than inside GameObject because objects are
        // data-driven (defined in XML). A subclass per object type would defeat that purpose.
        switchObj.OnCollide += () =>
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

    // Collision is handled here rather than in a separate system because each
    // interaction type has different consequences (damage, doors, death) that
    // require access to room-level state.  Sword–enemy collision lives in
    // PlayerSwingSwordState because it is tightly coupled to the swing animation.
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
                _player.GoInvulnerable(GameSettings.PlayerHitInvulDuration);

                if (_player.Health <= 0)
                    OnPlayerDied?.Invoke();
            }
        }

        foreach (var obj in Objects)
        {
            obj.Update(gameTime);

            if (_player.Collides(obj))
                obj.OnCollide?.Invoke();
        }
    }

    // Raised when the player's health reaches zero; PlayState subscribes to this.
    public event Action OnPlayerDied;

    public void Render(SpriteBatch spriteBatch)
    {
        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int w    = GameSettings.MapWidth;
        int h    = GameSettings.MapHeight;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var tile = _tilemap.GetTile(x, y);
                _tilemap.Tileset.GetTile(tile.GraphicId).Draw(spriteBatch, new Vector2(x * ts + offX, y * ts + offY), Color.White);
            }

        foreach (var doorway in Doorways)
            doorway.Draw(spriteBatch);

        foreach (var obj in Objects)
            obj.Draw(spriteBatch);

        foreach (var enemy in Enemies)
            enemy.Draw(spriteBatch);
    }
}
