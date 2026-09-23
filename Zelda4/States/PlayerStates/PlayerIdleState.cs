using Microsoft.Xna.Framework;
using Zelda4.Entities;
using Zelda4.Input;
using Zelda4.States.EntityStates;
using Zelda4.World;

namespace Zelda4.States.PlayerStates;

public class PlayerIdleState(Player player, Room room) : EntityStateBase(player)
{
    private readonly Player _player = player;
    private readonly Room _room = room;

    public override void Enter()
    {
        _player.SpriteOffset = new Vector2(0, GameSettings.PlayerSpriteOffsetY);
        _player.ChangeAnimation(AnimationKeys.Idle(_player.Direction));
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Left || GameController.Right ||
            GameController.Up   || GameController.Down)
        {
            _player.ChangeState(new PlayerWalkState(_player, _room));
        }

        if (GameController.SwingSword)
        {
            _player.ChangeState(new PlayerSwingSwordState(_player, _room));
        }
    }
}
