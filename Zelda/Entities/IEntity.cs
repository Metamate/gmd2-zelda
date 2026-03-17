using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.Entities;

public interface IEntity
{
    public Vector2 Position { get; set; }
    public Rectangle Bounds { get; }
    public bool Collidable { get; set; }
    public bool IsSolid { get; }
    public bool Active { get; set; }

    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
    bool Collides(IEntity other);
}
