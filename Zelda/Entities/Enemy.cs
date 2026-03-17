using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.States.EntityStates;
using Zelda.World;

namespace Zelda.Entities;

public class Enemy : Entity
{
    public EntityStateBase State { get; private set; }

    public void ChangeState(EntityStateBase newState)
    {
        State?.Exit();
        State = newState;
        State.Enter();
    }

    public void ProcessAI(Room room, GameTime gameTime)
    {
        State?.ProcessAI(room, gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        State?.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        State?.Draw(spriteBatch);
    }
}
