using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.World;

namespace Zelda.States.EntityStates;

public class EntityIdleState : EntityStateBase
{
    private float _waitDuration;
    private float _waitTimer;
    private bool _started;

    public EntityIdleState(Entity entity) : base(entity) { }

    public override void Enter()
    {
        Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));
        _started = false;
        _waitTimer = 0f;
    }

    // AI: pick a random wait duration on the first tick, then count down and resume walking.
    public override void ProcessAI(Room room, GameTime gameTime)
    {
        if (!_started)
        {
            _waitDuration = room.Random.Next(GameSettings.EntityMoveDurationMin, GameSettings.EntityMoveDurationMax);
            _started = true;
            return;
        }

        _waitTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_waitTimer > _waitDuration)
            Entity.ChangeState(new EntityWalkState(Entity));
    }
}
