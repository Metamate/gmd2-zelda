using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.World;

namespace Zelda.States.EntityStates;

public class EntityWalkState : EntityStateBase
{
    protected bool Bumped;
    private float _moveDuration;
    private float _movementTimer;

    private static readonly Direction[] Directions =
        [Direction.Left, Direction.Right, Direction.Up, Direction.Down];

    public EntityWalkState(Entity entity) : base(entity) { }

    public override void Enter()
    {
        Entity.ChangeAnimation($"walk-{Entity.Direction.ToName()}");
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Bumped = false;

        int tileSize = GameSettings.TileSize;
        int offsetX  = GameSettings.MapRenderOffsetX;
        int offsetY  = GameSettings.MapRenderOffsetY;
        int mapH     = GameSettings.MapHeight;

        Entity.Position += Entity.Direction.ToVector2() * Entity.WalkSpeed * dt;

        switch (Entity.Direction)
        {
            case Direction.Left:
                if (Entity.Position.X <= offsetX + tileSize)
                {
                    Entity.Position = Entity.Position with { X = offsetX + tileSize };
                    Bumped = true;
                }
                break;

            case Direction.Right:
                if (Entity.Position.X + Entity.Width >= GameSettings.VirtualWidth - tileSize * 2)
                {
                    Entity.Position = Entity.Position with { X = GameSettings.VirtualWidth - tileSize * 2 - Entity.Width };
                    Bumped = true;
                }
                break;

            case Direction.Up:
                if (Entity.Position.Y <= offsetY + tileSize - Entity.Height / 2f)
                {
                    Entity.Position = Entity.Position with { Y = offsetY + tileSize - Entity.Height / 2f };
                    Bumped = true;
                }
                break;

            case Direction.Down:
                float bottomEdge = offsetY + mapH * tileSize - tileSize;
                if (Entity.Position.Y + Entity.Height >= bottomEdge)
                {
                    Entity.Position = Entity.Position with { Y = bottomEdge - Entity.Height };
                    Bumped = true;
                }
                break;
        }
    }

    public override void ProcessAI(Room room, GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_moveDuration == 0f || Bumped)
        {
            _moveDuration = room.Random.Next(GameSettings.EntityMoveDurationMin, GameSettings.EntityMoveDurationMax);
            Entity.Direction = Directions[room.Random.Next(Directions.Length)];
            Entity.ChangeAnimation($"walk-{Entity.Direction.ToName()}");
        }
        else if (_movementTimer > _moveDuration)
        {
            _movementTimer = 0f;

            if (room.Random.Next(GameSettings.EntityIdleChance) == 0)
                Entity.ChangeState(new EntityIdleState(Entity));
            else
            {
                _moveDuration = room.Random.Next(GameSettings.EntityMoveDurationMin, GameSettings.EntityMoveDurationMax);
                Entity.Direction = Directions[room.Random.Next(Directions.Length)];
                Entity.ChangeAnimation($"walk-{Entity.Direction.ToName()}");
            }
        }

        _movementTimer += dt;
    }
}
