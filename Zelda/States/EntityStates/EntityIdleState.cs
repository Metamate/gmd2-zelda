using Microsoft.Xna.Framework;
using Zelda.Entities;
using Zelda.World;

namespace Zelda.States.EntityStates;

public class EntityIdleState : EntityStateBase
{
    private float _waitDuration;
    private float _waitTimer;

    public EntityIdleState(Entity entity) : base(entity) { }

    public override void Enter()
    {
        Entity.ChangeAnimation($"idle-{Entity.Direction.ToName()}");
        _waitDuration = 0f;
        _waitTimer = 0f;
    }

    // AI: wait a random duration, then resume walking.
    public override void ProcessAI(Room room, GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_waitDuration == 0f)
        {
            _waitDuration = room.Random.Next(1, 6); // 1–5 seconds
        }
        else
        {
            _waitTimer += dt;
            if (_waitTimer > _waitDuration)
                Entity.ChangeState(new EntityWalkState(Entity));
        }
    }
}
