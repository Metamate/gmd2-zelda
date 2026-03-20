using System;

namespace Zelda.Entities;

public enum AnimationKey
{
    WalkDown, WalkLeft, WalkRight, WalkUp,
    IdleDown, IdleLeft, IdleRight, IdleUp,
    SwordDown, SwordLeft, SwordRight, SwordUp,
}

public static class AnimationKeys
{
    public static AnimationKey Walk(Direction dir) => dir switch
    {
        Direction.Down  => AnimationKey.WalkDown,
        Direction.Left  => AnimationKey.WalkLeft,
        Direction.Right => AnimationKey.WalkRight,
        Direction.Up    => AnimationKey.WalkUp,
        _               => AnimationKey.WalkDown
    };

    public static AnimationKey Idle(Direction dir) => dir switch
    {
        Direction.Down  => AnimationKey.IdleDown,
        Direction.Left  => AnimationKey.IdleLeft,
        Direction.Right => AnimationKey.IdleRight,
        Direction.Up    => AnimationKey.IdleUp,
        _               => AnimationKey.IdleDown
    };

    public static AnimationKey Sword(Direction dir) => dir switch
    {
        Direction.Down  => AnimationKey.SwordDown,
        Direction.Left  => AnimationKey.SwordLeft,
        Direction.Right => AnimationKey.SwordRight,
        Direction.Up    => AnimationKey.SwordUp,
        _               => AnimationKey.SwordDown
    };

    public static AnimationKey Parse(string name) => name switch
    {
        "walk-down"  => AnimationKey.WalkDown,
        "walk-left"  => AnimationKey.WalkLeft,
        "walk-right" => AnimationKey.WalkRight,
        "walk-up"    => AnimationKey.WalkUp,
        "idle-down"  => AnimationKey.IdleDown,
        "idle-left"  => AnimationKey.IdleLeft,
        "idle-right" => AnimationKey.IdleRight,
        "idle-up"    => AnimationKey.IdleUp,
        "sword-down" => AnimationKey.SwordDown,
        "sword-left" => AnimationKey.SwordLeft,
        "sword-right"=> AnimationKey.SwordRight,
        "sword-up"   => AnimationKey.SwordUp,
        _ => throw new ArgumentException($"Unknown animation key: '{name}'")
    };
}
