using Microsoft.Xna.Framework;

namespace Zelda.Entities;

public class Player : Entity
{
    // Collision is restricted to the bottom half of the sprite to give a
    // top-down perspective feel — the "feet" collide, not the head.
    public Rectangle Hurtbox
    {
        get
        {
            int halfHeight = Height / 2;
            return new Rectangle((int)Position.X, (int)(Position.Y + halfHeight), Width, Height - halfHeight);
        }
    }

    public override bool Collides(IEntity other) =>
        Collidable && other.Collidable && Hurtbox.Intersects(other.Bounds);
}
