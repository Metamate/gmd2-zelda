using Microsoft.Xna.Framework;

namespace Zelda.Entities;

public enum Direction { Up, Down, Left, Right }

public static class DirectionExtensions
{
    public static string ToName(this Direction dir) => dir switch
    {
        Direction.Up => "up",
        Direction.Down => "down",
        Direction.Left => "left",
        Direction.Right => "right",
        _ => "down"
    };

    public static Vector2 ToVector2(this Direction dir) => dir switch
    {
        Direction.Left  => new(-1,  0),
        Direction.Right => new( 1,  0),
        Direction.Up    => new( 0, -1),
        Direction.Down  => new( 0,  1),
        _               => Vector2.Zero
    };
}
