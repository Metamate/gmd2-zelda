using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Audio;
using Zelda.Entities;
using Zelda.Input;
using Zelda.States.EntityStates;
using Zelda.World;

namespace Zelda.States.PlayerStates;

public class PlayerSwingSwordState : EntityStateBase
{
    private readonly Player _player;
    private readonly Dungeon _dungeon;
    private Rectangle _swordHitbox;

    public PlayerSwingSwordState(Player player, Dungeon dungeon) : base(player)
    {
        _player = player;
        _dungeon = dungeon;
    }

    public override void Enter()
    {
        _player.SpriteOffset = new Vector2(8, 5);

        // Build a sword hitbox in front of the player based on facing direction
        int px = (int)_player.Position.X;
        int py = (int)_player.Position.Y;

        _swordHitbox = _player.Direction switch
        {
            Direction.Left  => new Rectangle(px - 8,  py + 2, 8,  16),
            Direction.Right => new Rectangle(px + _player.Width, py + 2, 8, 16),
            Direction.Up    => new Rectangle(px, py - 8, 16, 8),
            Direction.Down  => new Rectangle(px, py + _player.Height, 16, 8),
            _               => new Rectangle(px, py + _player.Height, 16, 8)
        };

        _player.ChangeAnimation($"sword-{_player.Direction.ToName()}");

        SoundManager.PlaySound("sword");
        _player.Sprite.Refresh();
    }

    public override void Update(GameTime gameTime)
    {
        // Check for sword hits against all enemies in the current room
        foreach (var enemy in _dungeon.CurrentRoom.Enemies)
        {
            if (!enemy.IsDead && _swordHitbox.Intersects(enemy.Bounds))
            {
                enemy.Damage(1);
                SoundManager.PlaySound("hit-enemy");
            }
        }

        // When the non-looping swing animation has completed, return to idle
        // (animation is already advanced by Entity.Update called before State.Update)
        if (_player.Sprite != null && _player.Sprite.TimesPlayed > 0)
        {
            _player.Sprite.TimesPlayed = 0;
            _player.ChangeState(new PlayerIdleState(_player, _dungeon));
            return;
        }

        // Allow rapid re-swinging
        if (GameController.SwingSword)
        {
            _player.ChangeState(new PlayerSwingSwordState(_player, _dungeon));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Must call DrawSprite (non-virtual) rather than Draw, which would dispatch
        // back to Player.Draw → State.Draw → here → infinite recursion.
        _player.DrawSprite(spriteBatch);
    }
}
