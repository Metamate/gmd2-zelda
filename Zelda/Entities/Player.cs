using Microsoft.Xna.Framework;

namespace Zelda.Entities;

public class Player : Entity
{
    // The player's collision box uses the bottom half of the sprite for
    // top-down perspective, matching the Löve2D source.
    public override bool Collides(IEntity other)
    {
        var selfRect = new Rectangle(
            (int)Position.X,
            (int)(Position.Y + Height / 2f),
            Width,
            Height - Height / 2
        );
        return Collidable && other.Collidable && selfRect.Intersects(other.Bounds);
    }
}
