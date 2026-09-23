using GMDCore;
using Microsoft.Xna.Framework.Input;

namespace Zelda0.Input;

public static class GameController
{
    public static bool Confirm    => Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter);
}
