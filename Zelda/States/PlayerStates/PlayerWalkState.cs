using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.Input;
using Zelda.States.EntityStates;
using Zelda.World;

namespace Zelda.States.PlayerStates;

// Extends EntityWalkState: adds player input handling and doorway transition detection.
public class PlayerWalkState(Player player, Dungeon dungeon) : EntityWalkState(player)
{
    private readonly Player _player = player;
    private readonly Dungeon _dungeon = dungeon;

    public override void Enter()
    {
        _player.SpriteOffset = new Vector2(0, GameSettings.PlayerSpriteOffsetY);
    }

    public override void Update(GameTime gameTime)
    {
        // Map input to direction; transition to idle if no key is held
        Direction? dir = GameController.Left  ? Direction.Left  :
                         GameController.Right ? Direction.Right :
                         GameController.Up    ? Direction.Up    :
                         GameController.Down  ? Direction.Down  : null;

        if (dir is null)
        {
            _player.ChangeState(new PlayerIdleState(_player, _dungeon));
            return;
        }

        _player.Direction = dir.Value;
        _player.ChangeAnimation(AnimationKeys.Walk(dir.Value));

        if (GameController.SwingSword)
        {
            _player.ChangeState(new PlayerSwingSwordState(_player, _dungeon));
            return;
        }

        // Apply movement and wall collision (from EntityWalkState)
        base.Update(gameTime);

        // If we hit a wall, check if there is an open doorway to transition through
        if (Bumped)
            CheckDoorwayTransition(gameTime);
    }

    private void CheckDoorwayTransition(GameTime gameTime)
    {
        // Probe one step ahead in the current direction to check for an open doorway,
        // then restore position — movement only happens via BeginShift if found.
        float speed = GameSettings.PlayerWalkSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 step = _player.Direction.ToVector2() * speed;

        bool movingHorizontally = step.Y == 0;

        _player.Position += step;

        foreach (var doorway in _dungeon.CurrentRoom.Doorways)
        {
            if (_player.Collides(doorway) && doorway.IsOpen)
            {
                // Align the player to the centre of the doorway on the perpendicular axis
                if (movingHorizontally)
                    _player.Position = _player.Position with { Y = doorway.Position.Y + (doorway.Height - _player.Height) / 2f };
                else
                    _player.Position = _player.Position with { X = doorway.Position.X + (doorway.Width  - _player.Width)  / 2f };

                _dungeon.BeginShift(_player.Direction);
            }
        }

        _player.Position -= step;
    }
}
