using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.Input;

namespace Zelda.States.PlayerStates;

public class PlayerIdleState(Player player) : PlayerStateBase(player)
{
    public override void Enter()
    {
        SetAnimation("idle-animation");
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }
}