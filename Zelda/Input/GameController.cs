using GMDCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Zelda.Input;

public static class GameController
{
    public static bool Start       => Core.Input.Keyboard.IsKeyDown(Keys.Enter);
    public static bool SwingSword  => Core.Input.Keyboard.WasKeyJustPressed(Keys.Space);
    public static bool Left        => Core.Input.Keyboard.IsKeyDown(Keys.Left)  || Core.Input.Keyboard.IsKeyDown(Keys.A);
    public static bool Right       => Core.Input.Keyboard.IsKeyDown(Keys.Right) || Core.Input.Keyboard.IsKeyDown(Keys.D);
    public static bool Up          => Core.Input.Keyboard.IsKeyDown(Keys.Up)    || Core.Input.Keyboard.IsKeyDown(Keys.W);
    public static bool Down        => Core.Input.Keyboard.IsKeyDown(Keys.Down)  || Core.Input.Keyboard.IsKeyDown(Keys.S);
    public static bool Confirm     => Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter);
    public static bool Reset       => Core.Input.Keyboard.IsKeyDown(Keys.F);

    public static Vector2 Movement => new(
        Left ? -1 : Right ? 1 : 0,
        Up   ? -1 : Down  ? 1 : 0
    );
}
