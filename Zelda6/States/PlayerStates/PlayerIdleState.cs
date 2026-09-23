using Microsoft.Xna.Framework;
using Zelda6.Entities;
using Zelda6.Input;
using Zelda6.States.EntityStates;
using Zelda6.World;

namespace Zelda6.States.PlayerStates;

public class PlayerIdleState(Player player, Dungeon dungeon) : EntityStateBase(player)
{
    private readonly Player _player = player;
    private readonly Dungeon _dungeon = dungeon;

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
            _player.ChangeState(new PlayerWalkState(_player, _dungeon));
        }

        if (GameController.SwingSword)
        {
            _player.ChangeState(new PlayerSwingSwordState(_player, _dungeon));
        }
    }
}
