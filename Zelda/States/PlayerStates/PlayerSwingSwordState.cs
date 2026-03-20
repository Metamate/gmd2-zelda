using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Audio;
using Zelda.Graphics;
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
        _player.SpriteOffset = new Vector2(GameSettings.PlayerSwordOffsetX, GameSettings.PlayerSpriteOffsetY);

        // Build a sword hitbox in front of the player based on facing direction.
        // Reach = half a tile; the hitbox is one full tile wide on the perpendicular axis.
        int px = (int)_player.Position.X;
        int py = (int)_player.Position.Y;
        int reach = GameSettings.SwordReach;
        int ts = GameSettings.TileSize;

        int yOffset = GameSettings.SwordHitboxYOffset;
        _swordHitbox = _player.Direction switch
        {
            Direction.Left => new Rectangle(px - reach, py + yOffset, reach, ts),
            Direction.Right => new Rectangle(px + _player.Width, py + yOffset, reach, ts),
            Direction.Up => new Rectangle(px, py - reach, ts, reach),
            Direction.Down => new Rectangle(px, py + _player.Height, ts, reach),
            _ => new Rectangle(px, py + _player.Height, ts, reach)
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

    public override void DrawAt(SpriteBatch spriteBatch, Vector2 offset)
    {
        _player.DrawSprite(spriteBatch, offset);
        // DebugDraw.FillRect(spriteBatch, _swordHitbox, Color.Red * 0.4f); // sword hitbox
        // DebugDraw.FillRect(spriteBatch, _player.Hurtbox, Color.Green * 0.4f); // player hurtbox
    }
}
