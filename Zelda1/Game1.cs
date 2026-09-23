using GMDCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda1.Definitions;
using Zelda1.States.GameStates;

namespace Zelda1;

public class Game1 : Core
{
    private GameStateBase _currentState;
    // Re-exposed as public so game states can pass it to SpriteBatch.Begin().
    // 'new' widens the visibility from protected (in Core) to public here.
    public new Matrix ScreenScaleMatrix => base.ScreenScaleMatrix;

    public static SpriteFont DefaultFont { get; private set; }

    public Game1() : base("Zelda", GameSettings.WindowWidth, GameSettings.WindowHeight, GameSettings.VirtualWidth, GameSettings.VirtualHeight)
    {
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
        EntityDefinitions.LoadContent(Content);
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
