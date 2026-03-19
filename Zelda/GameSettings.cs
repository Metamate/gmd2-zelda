using Microsoft.Xna.Framework;

namespace Zelda;

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

    // Player
    public const int   PlayerWidth         = 16;
    public const int   PlayerHeight        = 22;
    public const int   PlayerWalkSpeed     = 60;
    public const int   PlayerStartHealth   = 6;     // three hearts × 2 per heart
    public const float PlayerSpriteOffsetY = 5f;    // sprite extends above collision box (perspective)
    public const float PlayerSwordOffsetX  = 8f;    // sword sprite (32px) centred over collision box (16px)

    // Door arch stencil mask — extra pixels the mask extends beyond the arch opening
    public const int DoorArchPadding = 6;

    // Sword reach — how far the hitbox extends in front of the player
    public const int SwordReach         = TileSize / 2;
    public const int SwordHitboxYOffset = 2;    // vertical nudge to centre hitbox on player

    // Invulnerability
    public const float PlayerHitInvulDuration = 1.5f;
    public const float InvulFlashInterval     = 0.06f;
    public const float InvulFlashAlpha        = 64f / 255f;

    // Room generation
    public const int RoomEnemyCount = 10;

    // Entity AI — movement timing and idle probability
    public const int EntityMoveDurationMin = 1;     // seconds (inclusive)
    public const int EntityMoveDurationMax = 6;     // seconds (exclusive upper bound for Next())
    public const int EntityIdleChance      = 3;     // 1-in-N chance to go idle after a move

    // HUD — hearts
    public const int HeartCount          = 3;
    public const int HeartHealthPerHeart = 2;
    public const int HeartFrameFull      = 4;
    public const int HeartFrameHalf      = 2;
    public const int HeartFrameEmpty     = 0;
    public const int HeartHudGap         = 1;   // px between heart icons
    public const int HeartHudOffsetY     = 2;   // px from top of screen

    // UI text layout
    public const float  UiTitleYOffset    = -10f;
    public const float  UiSubtitleSpacing =   5f;
    public const float  UiSubtitleScale   =  0.5f;
    public static readonly Color GameOverColor = new(175, 53, 42);

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
