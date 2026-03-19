using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.Input;
using Zelda.States.EntityStates;
using Zelda.World;

namespace Zelda.States.PlayerStates;

public class PlayerIdleState : EntityStateBase
{
    private readonly Player _player;
    private readonly Dungeon _dungeon;

    public PlayerIdleState(Player player, Dungeon dungeon) : base(player)
    {
        _player = player;
        _dungeon = dungeon;
    }

    public override void Enter()
    {
        _player.SpriteOffset = new Vector2(0, GameSettings.PlayerSpriteOffsetY);
        _player.ChangeAnimation($"idle-{_player.Direction.ToName()}");
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
