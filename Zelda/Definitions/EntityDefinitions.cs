using System.Collections.Generic;
using GMDCore.Graphics;
using Zelda.Entities;

namespace Zelda.Definitions;

// Mirrors entity_defs.lua: builds Animation dictionaries for each entity type.
// Frame indices are 1-based to match the Löve2D quad layout.
public static class EntityDefinitions
{
    public static Dictionary<string, Animation> CreatePlayerAnimations(
        TextureAtlas walkAtlas, TextureAtlas swordAtlas)
    {
        return new Dictionary<string, Animation>
        {
            // Walk animations (character_walk.png: 16x32 per frame, 4 cols x 4 rows)
            ["walk-down"]  = walkAtlas.CreateAnimation([1, 2, 3, 4],   0.15),
            ["walk-right"] = walkAtlas.CreateAnimation([5, 6, 7, 8],   0.15),
            ["walk-up"]    = walkAtlas.CreateAnimation([9, 10, 11, 12], 0.15),
            ["walk-left"]  = walkAtlas.CreateAnimation([13, 14, 15, 16], 0.155),

            // Idle animations (single frame)
            ["idle-down"]  = walkAtlas.CreateAnimation([1],  0.15),
            ["idle-right"] = walkAtlas.CreateAnimation([5],  0.15),
            ["idle-up"]    = walkAtlas.CreateAnimation([9],  0.15),
            ["idle-left"]  = walkAtlas.CreateAnimation([13], 0.15),

            // Sword swing (character_swing_sword.png: 32x32 per frame, non-looping)
            ["sword-down"]  = swordAtlas.CreateAnimation([1, 2, 3, 4],   0.05, loop: false),
            ["sword-up"]    = swordAtlas.CreateAnimation([5, 6, 7, 8],   0.05, loop: false),
            ["sword-right"] = swordAtlas.CreateAnimation([9, 10, 11, 12], 0.05, loop: false),
            ["sword-left"]  = swordAtlas.CreateAnimation([13, 14, 15, 16], 0.05, loop: false),
        };
    }

    // enemies all share the entities.png sheet (16x16 per frame, 12 cols)
    public static Dictionary<string, Animation> CreateEnemyAnimations(
        string type, TextureAtlas entityAtlas)
    {
        return type switch
        {
            "skeleton" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([10, 11, 12, 11], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([22, 23, 24, 23], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([34, 35, 36, 35], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([46, 47, 48, 47], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([11], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([23], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([35], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([47], 0.2),
            },
            "slime" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([49, 50, 51, 50], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([61, 62, 63, 62], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([73, 74, 75, 74], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([86, 86, 87, 86], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([50], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([62], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([74], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([86], 0.2),
            },
            "bat" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([52, 53, 54, 53], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([64, 65, 66, 65], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([76, 77, 78, 77], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([88, 89, 90, 89], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([52, 53, 54, 53], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([64, 65, 66, 65], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([76, 77, 78, 77], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([88, 89, 90, 89], 0.2),
            },
            "ghost" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([55, 56, 57, 56], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([67, 68, 69, 68], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([79, 80, 81, 80], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([91, 92, 93, 92], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([56], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([68], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([80], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([92], 0.2),
            },
            "spider" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([58, 59, 60, 59], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([70, 71, 72, 71], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([82, 83, 84, 83], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([94, 95, 96, 95], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([59], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([71], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([83], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([95], 0.2),
            },
            _ => new Dictionary<string, Animation>()
        };
    }
}
