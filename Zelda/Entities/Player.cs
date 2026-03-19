using Microsoft.Xna.Framework;

namespace Zelda.Entities;

public class Player : Entity
{
    // Collision is restricted to the bottom half of the sprite to give a
    // top-down perspective feel — the "feet" collide, not the head.
    public override bool Collides(IEntity other)
    {
        int halfHeight = Height / 2;
        var selfRect = new Rectangle(
            (int)Position.X,
            (int)(Position.Y + halfHeight),
            Width,
            Height - halfHeight
        );
        return Collidable && other.Collidable && selfRect.Intersects(other.Bounds);
    }
}
