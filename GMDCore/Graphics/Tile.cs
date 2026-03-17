namespace GMDCore.Graphics;

public readonly struct Tile(int graphicId = -1, int topperId = -1, bool isSolid = false)
{
    public static readonly Tile Empty = new();

    public int GraphicId { get; init; } = graphicId;
    public int TopperId { get; init; } = topperId;
    public bool IsSolid { get; init; } = isSolid;

    public bool HasTopper => TopperId >= 0;

    public Tile() : this(-1, -1, false) { }

    public Tile(int graphicId, bool isSolid) : this(graphicId, -1, isSolid) { }
}