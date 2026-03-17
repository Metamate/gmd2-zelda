using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.Input;
using Zelda.States.EntityStates;
using Zelda.World;

namespace Zelda.States.PlayerStates;

// Extends EntityWalkState: adds player input handling and doorway transition detection.
public class PlayerWalkState : EntityWalkState
{
    private readonly Player _player;
    private readonly Dungeon _dungeon;

    public PlayerWalkState(Player player, Dungeon dungeon) : base(player)
    {
        _player = player;
        _dungeon = dungeon;
    }

    public override void Enter()
    {
        _player.SpriteOffset = new Vector2(0, 5);
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Determine direction from input
        if (GameController.Left)
        {
            _player.Direction = Direction.Left;
            _player.ChangeAnimation("walk-left");
        }
        else if (GameController.Right)
        {
            _player.Direction = Direction.Right;
            _player.ChangeAnimation("walk-right");
        }
        else if (GameController.Up)
        {
            _player.Direction = Direction.Up;
            _player.ChangeAnimation("walk-up");
        }
        else if (GameController.Down)
        {
            _player.Direction = Direction.Down;
            _player.ChangeAnimation("walk-down");
        }
        else
        {
            _player.ChangeState(new PlayerIdleState(_player, _dungeon));
            return;
        }

        if (GameController.SwingSword)
        {
            _player.ChangeState(new PlayerSwingSwordState(_player, _dungeon));
            return;
        }

        // Apply movement and wall collision (from EntityWalkState)
        base.Update(gameTime);

        // If we hit a wall, check if there is an open doorway to transition through
        if (Bumped)
        {
            CheckDoorwayTransition(dt);
        }
    }

    private void CheckDoorwayTransition(float dt)
    {
        int speed = GameSettings.PlayerWalkSpeed;

        switch (_player.Direction)
        {
            case Direction.Left:
                _player.Position = _player.Position with { X = _player.Position.X - speed * dt };
                foreach (var doorway in _dungeon.CurrentRoom.Doorways)
                {
                    if (_player.Collides(doorway) && doorway.IsOpen)
                    {
                        _player.Position = _player.Position with { Y = doorway.Position.Y + 4 };
                        _dungeon.BeginShift(Direction.Left);
                    }
                }
                _player.Position = _player.Position with { X = _player.Position.X + speed * dt };
                break;

            case Direction.Right:
                _player.Position = _player.Position with { X = _player.Position.X + speed * dt };
                foreach (var doorway in _dungeon.CurrentRoom.Doorways)
                {
                    if (_player.Collides(doorway) && doorway.IsOpen)
                    {
                        _player.Position = _player.Position with { Y = doorway.Position.Y + 4 };
                        _dungeon.BeginShift(Direction.Right);
                    }
                }
                _player.Position = _player.Position with { X = _player.Position.X - speed * dt };
                break;

            case Direction.Up:
                _player.Position = _player.Position with { Y = _player.Position.Y - speed * dt };
                foreach (var doorway in _dungeon.CurrentRoom.Doorways)
                {
                    if (_player.Collides(doorway) && doorway.IsOpen)
                    {
                        _player.Position = _player.Position with { X = doorway.Position.X + 8 };
                        _dungeon.BeginShift(Direction.Up);
                    }
                }
                _player.Position = _player.Position with { Y = _player.Position.Y + speed * dt };
                break;

            case Direction.Down:
                _player.Position = _player.Position with { Y = _player.Position.Y + speed * dt };
                foreach (var doorway in _dungeon.CurrentRoom.Doorways)
                {
                    if (_player.Collides(doorway) && doorway.IsOpen)
                    {
                        _player.Position = _player.Position with { X = doorway.Position.X + 8 };
                        _dungeon.BeginShift(Direction.Down);
                    }
                }
                _player.Position = _player.Position with { Y = _player.Position.Y - speed * dt };
                break;
        }
    }
}
