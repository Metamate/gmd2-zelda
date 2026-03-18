using Microsoft.Xna.Framework;
using Zelda.World;

namespace Zelda.Entities;

public class Enemy : Entity
{
    public void ProcessAI(Room room, GameTime gameTime) => State?.ProcessAI(room, gameTime);
}
