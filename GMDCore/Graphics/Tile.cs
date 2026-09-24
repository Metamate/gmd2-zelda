namespace GMDCore.Graphics;

// One cell of a tilemap: which tileset graphic to draw, and whether it blocks movement.
public readonly struct Tile(int graphicId = -1, bool isSolid = false)
{
    public static readonly Tile Empty = new();

    public int GraphicId { get; init; } = graphicId;
    public bool IsSolid { get; init; } = isSolid;

    public bool IsEmpty => GraphicId < 0;

    public Tile() : this(-1, false) { }
}