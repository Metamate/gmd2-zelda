using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GMDCore.Input;

public class MouseInfo
{
    public MouseState PreviousState { get; private set; }
    public MouseState CurrentState { get; private set; }

    public MouseInfo()
    {
        PreviousState = new MouseState();
        CurrentState = Mouse.GetState();
    }

    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = Mouse.GetState();
    }

    // The position in window coordinates, not in the game's virtual resolution.
    public int X => CurrentState.X;
    public int Y => CurrentState.Y;
    public Point Position => CurrentState.Position;

    public bool IsLeftButtonDown
        => CurrentState.LeftButton == ButtonState.Pressed;

    public bool WasLeftButtonJustPressed
        => CurrentState.LeftButton == ButtonState.Pressed && PreviousState.LeftButton == ButtonState.Released;
}