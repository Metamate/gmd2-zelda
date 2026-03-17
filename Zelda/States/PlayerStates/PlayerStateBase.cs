using System;
using System.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Entities;
using Zelda.Input;

namespace Zelda.States.PlayerStates;

public abstract class PlayerStateBase
{
    protected const float MoveSpeed = 100f;
    protected const float Gravity = 1000f;
    protected const int CollisionInset = 1;
    protected const float CoyoteTime = 0.1f;

    protected Player Player { get; }

    protected PlayerStateBase(Player player)
    {
        Player = player;
    }

    protected void SetAnimation(string name)
    {
        var animation = Player.Atlas.GetAnimation(name);
        if (Player.Sprite == null)
            Player.Sprite = new AnimatedSprite(animation);
        else
            Player.Sprite.Play(animation);
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Update(GameTime gameTime)
    {
        Player.Sprite?.Update(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Player.Sprite.Draw(spriteBatch, Player.Position);
    }
}