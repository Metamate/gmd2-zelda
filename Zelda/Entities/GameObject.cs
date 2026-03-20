using System;
using System.Collections.Generic;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.Entities;

// A non-entity interactive object in the room (e.g., a floor switch).
// Unlike Entity, a GameObject has no health, AI, or state machine.
public class GameObject(
    string type,
    TextureAtlas atlas,
    Dictionary<string, int> stateFrames,
    string defaultState,
    int width,
    int height) : IEntity
{
    public string Type { get; set; } = type;

    // Current state key (e.g., "unpressed" / "pressed")
    public string State { get; set; } = defaultState;

    // Maps state name to the frame index in the object's atlas
    private readonly Dictionary<string, int> _stateFrames = stateFrames;

    private readonly TextureAtlas _atlas = atlas;

    // Callback invoked when the player collides with this object.
    // Use += to subscribe; never assign with = (would silently discard prior handlers).
    public Action OnCollide { get; set; }

    public Vector2 Position { get; set; }
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

    public bool Collidable { get; set; } = true;
    public bool IsSolid => false;
    public bool Active { get; set; } = true;

    public void Update(GameTime gameTime) { }

    public void Draw(SpriteBatch spriteBatch)
    {
        int frame = _stateFrames[State];
        _atlas.GetRegion($"frame_{frame}").Draw(spriteBatch, Position, Color.White);
    }

    public bool Collides(IEntity other) =>
        Collidable && other.Collidable && Bounds.Intersects(other.Bounds);
}
