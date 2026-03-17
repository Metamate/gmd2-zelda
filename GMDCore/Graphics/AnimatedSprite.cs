using System;
using Microsoft.Xna.Framework;

namespace GMDCore.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _animation;

    public Animation Animation
    {
        get => _animation;
        set => Play(value);
    }

    public void Play(Animation animation)
    {
        if (_animation == animation) return;

        _animation = animation;
        _currentFrame = 0;
        _elapsed = TimeSpan.Zero;
        Region = _animation.Frames[0];
    }

    public AnimatedSprite() { }

    public AnimatedSprite(Animation animation)
    {
        Animation = animation;
    }

    public void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= _animation.Delay)
        {
            _elapsed -= _animation.Delay;
            _currentFrame++;

            if (_currentFrame >= _animation.Frames.Count)
            {
                _currentFrame = 0;
            }

            Region = _animation.Frames[_currentFrame];
        }
    }
}