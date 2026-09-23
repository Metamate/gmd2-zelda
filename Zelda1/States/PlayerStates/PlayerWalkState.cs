using Microsoft.Xna.Framework;
using Zelda1.Entities;
using Zelda1.Input;
using Zelda1.States.EntityStates;
using Zelda1.World;

namespace Zelda1.States.PlayerStates;

// Extends EntityWalkState: adds player input handling.
public class PlayerWalkState(Player player, Room room) : EntityWalkState(player)
{
    private readonly Player _player = player;
    private readonly Room _room = room;

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
            _player.ChangeState(new PlayerIdleState(_player, _room));
            return;
        }

        _player.Direction = dir.Value;
        _player.ChangeAnimation(AnimationKeys.Walk(dir.Value));

        // Apply movement and wall collision (from EntityWalkState)
        base.Update(gameTime);
    }
}
