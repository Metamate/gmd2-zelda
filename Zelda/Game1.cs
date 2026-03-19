using GMDCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Audio;
using Zelda.Definitions;
using Zelda.States.GameStates;
using Zelda.World;

namespace Zelda;

public class Game1 : Core
{
    private GameStateBase _currentState;
    public new Matrix ScreenScaleMatrix => base.ScreenScaleMatrix;

    public static SpriteFont DefaultFont { get; private set; }

    public Game1() : base("Zelda", GameSettings.WindowWidth, GameSettings.WindowHeight, GameSettings.VirtualWidth, GameSettings.VirtualHeight)
    {
        // Stencil buffer is required for the door-arch clipping effect.
        Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
    }

    protected override void Initialize()
    {
        base.Initialize();
        SetState(new StartState(this));
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        DefaultFont = Content.Load<SpriteFont>("fonts/font");
        SoundManager.LoadContent(Content);
        Doorway.LoadContent(Content);
        EntityDefinitions.LoadContent(Content);
        GameObjectDefinitions.LoadContent(Content);
    }

    public void SetState(GameStateBase newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    protected override void Update(GameTime gameTime)
    {
        _currentState?.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _currentState?.Draw(SpriteBatch);
        base.Draw(gameTime);
    }
}
