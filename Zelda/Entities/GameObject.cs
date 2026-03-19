using System;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.Entities;

// A non-entity interactive object in the room (e.g., a floor switch).
// Unlike Entity, a GameObject has no health, AI, or state machine.
public class GameObject : IEntity
{
    public string Type { get; set; }

    // Current state key (e.g., "unpressed" / "pressed")
    public string State { get; set; }

    // Maps state name to the frame index in the object's atlas
    private readonly System.Collections.Generic.Dictionary<string, int> _stateFrames;

    private readonly TextureAtlas _atlas;

    // Callback invoked when the player collides with this object
    public Action OnCollide { get; set; } = () => { };

    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public bool Collidable { get; set; } = true;
    public bool IsSolid => false;
    public bool Active { get; set; } = true;

    public GameObject(
        string type,
        TextureAtlas atlas,
        System.Collections.Generic.Dictionary<string, int> stateFrames,
        string defaultState,
        int width,
        int height)
    {
        Type = type;
        _atlas = atlas;
        _stateFrames = stateFrames;
        State = defaultState;
        Width = width;
        Height = height;
    }

    public void Update(GameTime gameTime) { }

    public void Draw(SpriteBatch spriteBatch)
    {
        int frame = _stateFrames[State];
        _atlas.GetRegion($"frame_{frame}").Draw(spriteBatch, Position, Color.White);
    }

    public bool Collides(IEntity other) =>
        Collidable && other.Collidable && Bounds.Intersects(other.Bounds);
}
