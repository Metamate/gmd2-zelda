using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Entities;
using Zelda.World;

namespace Zelda.States.EntityStates;

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

    // Called each frame for AI-controlled entities; players override or ignore this.
    public virtual void ProcessAI(Room room, GameTime gameTime) { }

    public void Draw(SpriteBatch spriteBatch) => DrawAt(spriteBatch, Vector2.Zero);

    public virtual void DrawAt(SpriteBatch spriteBatch, Vector2 offset)
    {
        Entity.DrawSprite(spriteBatch, offset);
    }
}
