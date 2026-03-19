using System.Collections.Generic;
using GMDCore.Graphics;
using Zelda.Entities;

namespace Zelda.Definitions;

// Builds Animation dictionaries for each entity type.
public static class EntityDefinitions
{
    public static Dictionary<string, Animation> CreatePlayerAnimations(
        TextureAtlas walkAtlas, TextureAtlas swordAtlas)
    {
        return new Dictionary<string, Animation>
        {
            // Walk animations (character_walk.png: 16x32 per frame, 4 cols x 4 rows)
            ["walk-down"]  = walkAtlas.CreateAnimation([0, 1, 2, 3],   0.15),
            ["walk-right"] = walkAtlas.CreateAnimation([4, 5, 6, 7],   0.15),
            ["walk-up"]    = walkAtlas.CreateAnimation([8, 9, 10, 11], 0.15),
            ["walk-left"]  = walkAtlas.CreateAnimation([12, 13, 14, 15], 0.155),

            // Idle animations (single frame)
            ["idle-down"]  = walkAtlas.CreateAnimation([0],  0.15),
            ["idle-right"] = walkAtlas.CreateAnimation([4],  0.15),
            ["idle-up"]    = walkAtlas.CreateAnimation([8],  0.15),
            ["idle-left"]  = walkAtlas.CreateAnimation([12], 0.15),

            // Sword swing (character_swing_sword.png: 32x32 per frame, non-looping)
            ["sword-down"]  = swordAtlas.CreateAnimation([0, 1, 2, 3],   0.05, loop: false),
            ["sword-up"]    = swordAtlas.CreateAnimation([4, 5, 6, 7],   0.05, loop: false),
            ["sword-right"] = swordAtlas.CreateAnimation([8, 9, 10, 11], 0.05, loop: false),
            ["sword-left"]  = swordAtlas.CreateAnimation([12, 13, 14, 15], 0.05, loop: false),
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
                ["walk-down"]  = entityAtlas.CreateAnimation([9, 10, 11, 10], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([21, 22, 23, 22], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([33, 34, 35, 34], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([45, 46, 47, 46], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([10], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([22], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([34], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([46], 0.2),
            },
            "slime" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([48, 49, 50, 49], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([60, 61, 62, 61], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([72, 73, 74, 73], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([85, 85, 86, 85], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([49], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([61], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([73], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([85], 0.2),
            },
            "bat" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([51, 52, 53, 52], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([63, 64, 65, 64], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([75, 76, 77, 76], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([87, 88, 89, 88], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([51, 52, 53, 52], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([63, 64, 65, 64], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([75, 76, 77, 76], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([87, 88, 89, 88], 0.2),
            },
            "ghost" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([54, 55, 56, 55], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([66, 67, 68, 67], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([78, 79, 80, 79], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([90, 91, 92, 91], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([55], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([67], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([79], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([91], 0.2),
            },
            "spider" => new Dictionary<string, Animation>
            {
                ["walk-down"]  = entityAtlas.CreateAnimation([57, 58, 59, 58], 0.2),
                ["walk-left"]  = entityAtlas.CreateAnimation([69, 70, 71, 70], 0.2),
                ["walk-right"] = entityAtlas.CreateAnimation([81, 82, 83, 82], 0.2),
                ["walk-up"]    = entityAtlas.CreateAnimation([93, 94, 95, 94], 0.2),
                ["idle-down"]  = entityAtlas.CreateAnimation([58], 0.2),
                ["idle-left"]  = entityAtlas.CreateAnimation([70], 0.2),
                ["idle-right"] = entityAtlas.CreateAnimation([82], 0.2),
                ["idle-up"]    = entityAtlas.CreateAnimation([94], 0.2),
            },
            _ => new Dictionary<string, Animation>()
        };
    }
}
