namespace GMDCore.Input;

public class InputManager
{
    public KeyboardInfo Keyboard { get; private set; } = new KeyboardInfo();

    public void Update()
    {
        Keyboard.Update();
    }
}