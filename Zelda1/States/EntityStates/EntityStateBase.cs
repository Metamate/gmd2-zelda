using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda1.Entities;

namespace Zelda1.States.EntityStates;

public abstract class EntityStateBase
{
    protected Entity Entity { get; }

    protected EntityStateBase(Entity entity)
    {
        Entity = entity;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    // Entity.Update (called from Enemy.Update / Player.Update via base) already advances
    // the sprite animation, so states should not call Sprite.Update again.
    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Entity.DrawSprite(spriteBatch);
    }
}
