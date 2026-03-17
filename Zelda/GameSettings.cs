namespace Zelda;

public static class GameSettings
{
    public const int VirtualWidth = 384;
    public const int VirtualHeight = 216;

    public const int TileSize = 16;

    public const int PlayerWalkSpeed = 60;

    public const int MapWidth = VirtualWidth / TileSize - 2;
    public const int MapHeight = VirtualHeight / TileSize - 2;

    public const int MapRenderOffsetX = (VirtualWidth - MapWidth * TileSize) / 2;
    public const int MapRenderOffsetY = (VirtualHeight - MapHeight * TileSize) / 2;

    // Tile IDs (1-based, matching tilesheet quad layout)
    public const int TileTopLeftCorner = 4;
    public const int TileTopRightCorner = 5;
    public const int TileBottomLeftCorner = 23;
    public const int TileBottomRightCorner = 24;
    public const int TileEmpty = 19;

    public static readonly int[] TileFloors =
    [
        7, 8, 9, 10, 11, 12, 13,
        26, 27, 28, 29, 30, 31, 32,
        45, 46, 47, 48, 49, 50, 51,
        64, 65, 66, 67, 68, 69, 70,
        88, 89, 107, 108
    ];

    public static readonly int[] TileTopWalls = [58, 59, 60];
    public static readonly int[] TileBottomWalls = [79, 80, 81];
    public static readonly int[] TileLeftWalls = [77, 96, 115];
    public static readonly int[] TileRightWalls = [78, 97, 116];
}
