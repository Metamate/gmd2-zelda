using Microsoft.Xna.Framework;
using Zelda.World;

namespace Zelda.Entities;

// Enemy is a named subtype of Entity that exposes AI ticking.
// Entity deliberately doesn't have ProcessAI because the player is also
// an Entity but is driven by input, not AI.
public class Enemy : Entity
{
    public void ProcessAI(Room room, GameTime gameTime) => State?.ProcessAI(room, gameTime);
}
