using System.Collections.Generic;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda1.States.EntityStates;

namespace Zelda1.Entities;

public abstract class Entity : IEntity
{
    public Direction Direction { get; set; } = Direction.Down;

    public Dictionary<AnimationKey, Animation> Animations { get; } = new();

    public AnimatedSprite Sprite { get; set; }

    public int WalkSpeed { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    // Offset applied when drawing the sprite relative to the collision position.
    // The sprite is drawn at (Position - SpriteOffset), so a positive Y offset
    // means the sprite extends above the collision box (perspective look).
    public Vector2 SpriteOffset { get; set; } = Vector2.Zero;

    public Vector2 Position { get; set; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public bool Collidable { get; set; } = true;

    public bool IsSolid => false;

    public bool Active { get; set; } = true;

    public EntityStateBase State { get; private set; }

    public void ChangeState(EntityStateBase newState)
    {
        State?.Exit();
        State = newState;
        State.Enter();
    }

    public void ChangeAnimation(AnimationKey key)
    {
        if (Sprite == null)
            Sprite = new AnimatedSprite(Animations[key]);
        else
            Sprite.Play(Animations[key]);
    }

    public virtual bool Collides(IEntity other) =>
        Collidable && other.Collidable && Bounds.Intersects(other.Bounds);

    public virtual void Update(GameTime gameTime)
    {
        Sprite?.Update(gameTime);
        State?.Update(gameTime);
    }

    public void DrawSprite(SpriteBatch spriteBatch)
    {
        Sprite?.Region?.Draw(spriteBatch, Position - SpriteOffset, Color.White);
    }

    public void Draw(SpriteBatch spriteBatch) => State?.Draw(spriteBatch);
}
