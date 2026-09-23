using Microsoft.Xna.Framework;

namespace Zelda0;

public static class GameSettings
{
    public const int WindowWidth   = 1280;
    public const int WindowHeight  = 720;

    public const int VirtualWidth  = 384;
    public const int VirtualHeight = 216;

    public const int TileSize = 16;

    // Map layout — total tiles that fit on screen minus 1 border tile on each side
    public const int MapWidth  = VirtualWidth  / TileSize - 2;
    public const int MapHeight = VirtualHeight / TileSize - 2;

    public const int MapRenderOffsetX = (VirtualWidth  - MapWidth  * TileSize) / 2;
    public const int MapRenderOffsetY = (VirtualHeight - MapHeight * TileSize) / 2;

    // UI text layout
    public const float  UiTitleYOffset    = -10f;
    public const float  UiSubtitleSpacing =   5f;
    public const float  UiSubtitleScale   =  0.5f;

    // Tile IDs (0-based)
    public const int TileTopLeftCorner     =  3;
    public const int TileTopRightCorner    =  4;
    public const int TileBottomLeftCorner  = 22;
    public const int TileBottomRightCorner = 23;
    public const int TileEmpty             = 18;

    public static readonly int[] TileFloors =
    [
        6, 7, 8, 9, 10, 11, 12,
        25, 26, 27, 28, 29, 30, 31,
        44, 45, 46, 47, 48, 49, 50,
        63, 64, 65, 66, 67, 68, 69,
        87, 88, 106, 107
    ];

    public static readonly int[] TileTopWalls    = [57, 58, 59];
    public static readonly int[] TileBottomWalls = [78, 79, 80];
    public static readonly int[] TileLeftWalls   = [76, 95, 114];
    public static readonly int[] TileRightWalls  = [77, 96, 115];
}
