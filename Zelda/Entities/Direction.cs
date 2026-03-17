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
}
