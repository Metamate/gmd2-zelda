using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.States.PlayerStates;

namespace Zelda.Entities;

public class Player : IEntity
{
    public const int HitboxInset = 2;

    public TextureAtlas Atlas { get; }
    public PlayerStateBase State { get; private set; }
    public AnimatedSprite Sprite { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public int Score { get; set; }

    public Rectangle Bounds => new(
        (int)Position.X + HitboxInset,
        (int)Position.Y,
        (int)Sprite.Width - (HitboxInset * 2),
        (int)Sprite.Height
    );

    public bool Collidable { get; set; } = true;
    public bool IsSolid => true;
    public bool Active { get; set; } = true;

    public Player(TextureAtlas textureAtlas)
    {
        Atlas = textureAtlas;
        ChangeState(new PlayerIdleState(this));
    }

    public void ChangeState(PlayerStateBase newState)
    {
        State?.Exit();
        State = newState;
        State.Enter();
    }

    public void Update(GameTime gameTime)
    {
        State.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        State.Draw(spriteBatch);
    }

    public bool Collides(IEntity other)
    {
        return Collidable && other.Collidable && Bounds.Intersects(other.Bounds);
    }
}
