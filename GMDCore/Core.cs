using System;
using GMDCore.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GMDCore;

public class Core : Game
{
    private int _virtualWidth;
    private int _virtualHeight;
    protected GraphicsDeviceManager Graphics;
    protected Matrix ScreenScaleMatrix;
    public SpriteBatch SpriteBatch { get; set; }
    public static InputManager Input { get; set; } = new();

    public Core(string title, int windowWidth, int windowHeight, int virtualWidth, int virtualHeight)
    {
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
        Graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = windowWidth,
            PreferredBackBufferHeight = windowHeight,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = title;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (s, e) => UpdateScreenScaleMatrix();
    }

    protected override void Initialize()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        UpdateScreenScaleMatrix();
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Input.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    private void UpdateScreenScaleMatrix()
    {
        float screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        float screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float currentWidth, currentHeight;

        if (screenWidth / _virtualWidth > screenHeight / _virtualHeight)
        {
            float aspect = screenHeight / _virtualHeight;
            currentWidth = aspect * _virtualWidth;
            currentHeight = screenHeight;
        }
        else
        {
            float aspect = screenWidth / _virtualWidth;
            currentWidth = screenWidth;
            currentHeight = aspect * _virtualHeight;
        }

        ScreenScaleMatrix = Matrix.CreateScale(currentWidth / _virtualWidth, currentHeight / _virtualHeight, 1);

        GraphicsDevice.Viewport = new()
        {
            X = (int)(screenWidth / 2 - currentWidth / 2),
            Y = (int)(screenHeight / 2 - currentHeight / 2),
            Width = (int)currentWidth,
            Height = (int)currentHeight,
            MinDepth = 0,
            MaxDepth = 1,
        };
    }
}
