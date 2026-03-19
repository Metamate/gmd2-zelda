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
        _player.SpriteOffset = new Vector2(0, GameSettings.PlayerSpriteOffsetY);
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
        // Convert direction to a movement step vector for the probe
        float speed = GameSettings.PlayerWalkSpeed * dt;
        Vector2 step = _player.Direction switch
        {
            Direction.Left  => new Vector2(-speed,  0),
            Direction.Right => new Vector2( speed,  0),
            Direction.Up    => new Vector2(0, -speed),
            Direction.Down  => new Vector2(0,  speed),
            _               => Vector2.Zero
        };

        bool horizontal = step.Y == 0;

        _player.Position += step;

        foreach (var doorway in _dungeon.CurrentRoom.Doorways)
        {
            if (_player.Collides(doorway) && doorway.IsOpen)
            {
                // Align the player to the centre of the doorway on the perpendicular axis
                if (horizontal)
                    _player.Position = _player.Position with { Y = doorway.Position.Y + 4 };
                else
                    _player.Position = _player.Position with { X = doorway.Position.X + 8 };

                _dungeon.BeginShift(_player.Direction);
            }
        }

        _player.Position -= step;
    }
}
