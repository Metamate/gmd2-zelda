namespace GMDCore.Input;

public class InputManager
{
    public KeyboardInfo Keyboard { get; private set; } = new KeyboardInfo();
    public MouseInfo Mouse { get; private set; } = new MouseInfo();

    public void Update()
    {
        Keyboard.Update();
        Mouse.Update();
    }
}