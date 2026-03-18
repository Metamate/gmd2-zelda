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

        int tileSize  = GameSettings.TileSize;
        int offsetX   = GameSettings.MapRenderOffsetX;
        int offsetY   = GameSettings.MapRenderOffsetY;
        int mapH      = GameSettings.MapHeight;
        int vHeight   = GameSettings.VirtualHeight;

        switch (Entity.Direction)
        {
            case Direction.Left:
                Entity.Position = Entity.Position with { X = Entity.Position.X - Entity.WalkSpeed * dt };
                if (Entity.Position.X <= offsetX + tileSize)
                {
                    Entity.Position = Entity.Position with { X = offsetX + tileSize };
                    Bumped = true;
                }
                break;

            case Direction.Right:
                Entity.Position = Entity.Position with { X = Entity.Position.X + Entity.WalkSpeed * dt };
                if (Entity.Position.X + Entity.Width >= GameSettings.VirtualWidth - tileSize * 2)
                {
                    Entity.Position = Entity.Position with { X = GameSettings.VirtualWidth - tileSize * 2 - Entity.Width };
                    Bumped = true;
                }
                break;

            case Direction.Up:
                Entity.Position = Entity.Position with { Y = Entity.Position.Y - Entity.WalkSpeed * dt };
                if (Entity.Position.Y <= offsetY + tileSize - Entity.Height / 2f)
                {
                    Entity.Position = Entity.Position with { Y = offsetY + tileSize - Entity.Height / 2f };
                    Bumped = true;
                }
                break;

            case Direction.Down:
                Entity.Position = Entity.Position with { Y = Entity.Position.Y + Entity.WalkSpeed * dt };
                float bottomEdge = vHeight - (vHeight - mapH * tileSize) + offsetY - tileSize;
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
            _moveDuration = room.Random.Next(1, 6);
            Entity.Direction = Directions[room.Random.Next(Directions.Length)];
            Entity.ChangeAnimation($"walk-{Entity.Direction.ToName()}");
        }
        else if (_movementTimer > _moveDuration)
        {
            _movementTimer = 0f;

            if (room.Random.Next(3) == 0)
                Entity.ChangeState(new EntityIdleState(Entity));
            else
            {
                _moveDuration = room.Random.Next(1, 6);
                Entity.Direction = Directions[room.Random.Next(Directions.Length)];
                Entity.ChangeAnimation($"walk-{Entity.Direction.ToName()}");
            }
        }

        _movementTimer += dt;
    }
}
