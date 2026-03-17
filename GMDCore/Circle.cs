using Microsoft.Xna.Framework;

namespace GMDCore;

public readonly record struct Circle
{
    private static readonly Circle s_empty = new();
    public readonly int X;
    public readonly int Y;
    public readonly int Radius;
    public readonly Point Location => new(X, Y);
    public static Circle Empty => s_empty;
    public readonly bool IsEmpty => X == 0 && Y == 0 && Radius == 0;
    public readonly int Top => Y - Radius;
    public readonly int Bottom => Y + Radius;
    public readonly int Left => X - Radius;
    public readonly int Right => X + Radius;

    public Circle(int x, int y, int radius)
    {
        X = x;
        Y = y;
        Radius = radius;
    }

    public Circle(Point location, int radius)
    {
        X = location.X;
        Y = location.Y;
        Radius = radius;
    }

    public bool Intersects(Circle other)
    {
        int radiiSquared = (Radius + other.Radius) * (Radius + other.Radius);
        float distanceSquared = Vector2.DistanceSquared(Location.ToVector2(), other.Location.ToVector2());
        return distanceSquared < radiiSquared;
    }
}