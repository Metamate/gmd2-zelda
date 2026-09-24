using System;
using GMDCore.Tweening;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda7.Audio;
using Zelda7.Entities;
using Zelda7.Graphics;

namespace Zelda7.World;

// Manages the current and next Room during play, including the camera-shift
// transition between rooms.
public class Dungeon
{
    private readonly Player _player;
    private readonly Func<Room> _roomFactory;

    public Room CurrentRoom { get; private set; }
    private Room _nextRoom;

    // Camera whose Transform is combined with the screen-scale matrix each frame.
    // During a shift it is tweened from (0,0) to _shiftTarget; everything rendered through
    // the combined matrix moves automatically — no per-object offset arithmetic needed.
    private readonly Camera _camera = new();
    private Vector2 _shiftTarget;

    // The next room is placed at _shiftTarget and stays there; the camera tween
    // makes it slide into view.
    private Vector2 _nextRoomOffset;

    private readonly TweenManager _tweens = new();
    private bool _shifting;
    private Direction _shiftDirection;

    // Fired when PlayerDied event is forwarded from Room
    public event Action OnPlayerDied;

    public Dungeon(Player player, Func<Room> roomFactory)
    {
        _player = player;
        _roomFactory = roomFactory;
        CurrentRoom = CreateRoom();
    }

    private Room CreateRoom()
    {
        var room = _roomFactory();
        room.OnPlayerDied += () => OnPlayerDied?.Invoke();
        return room;
    }

    public void BeginShift(Direction direction)
    {
        if (_shifting) return;

        _shifting = true;
        _shiftDirection = direction;
        _nextRoom = CreateRoom();

        // All doors in the incoming room start open so the player walks through
        foreach (var d in _nextRoom.Doorways)
            d.IsOpen = true;

        int vw   = GameSettings.VirtualWidth;
        int vh   = GameSettings.VirtualHeight;
        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapW = GameSettings.MapWidth;
        int mapH = GameSettings.MapHeight;

        _camera.Position = Vector2.Zero;
        _shiftTarget = direction switch
        {
            Direction.Left  => new Vector2(-vw, 0),
            Direction.Right => new Vector2( vw, 0),
            Direction.Up    => new Vector2(0, -vh),
            Direction.Down  => new Vector2(0,  vh),
            _ => Vector2.Zero
        };

        // The next room sits at _shiftTarget and never moves; the camera tween
        // brings it into view.
        _nextRoomOffset = _shiftTarget;

        // Where the player walks to through the doorway
        Vector2 playerStart = _player.Position;
        Vector2 playerEnd = direction switch
        {
            Direction.Right => new Vector2(vw  + offX + ts,                             _player.Position.Y),
            Direction.Left  => new Vector2(-vw + offX + mapW * ts - ts - _player.Width, _player.Position.Y),
            Direction.Down  => new Vector2(_player.Position.X,                           vh  + offY + _player.Height / 2f),
            Direction.Up    => new Vector2(_player.Position.X,                          -vh  + offY + mapH * ts - ts - _player.Height),
            _ => _player.Position
        };

        // Move the camera to the next room and walk the player through the doorway, both
        // over the same time. Everything rendered through the combined camera and
        // screen-scale matrix shifts with the camera. When the tween ends, the shift is done.
        _tweens.Tween(GameSettings.RoomShiftDuration)
            .Add(t => _camera.Position = Vector2.Lerp(Vector2.Zero, _shiftTarget, t), 0f, 1f)
            .Add(t => _player.Position = Vector2.Lerp(playerStart, playerEnd, t), 0f, 1f)
            .Finish(FinishShift);
    }

    private void FinishShift()
    {
        _shifting = false;
        _camera.Position = Vector2.Zero;

        CurrentRoom = _nextRoom;
        _nextRoom = null;

        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapW = GameSettings.MapWidth;
        int mapH = GameSettings.MapHeight;

        // Snap player to the correct entry point in the new room
        switch (_shiftDirection)
        {
            case Direction.Left:
                _player.Position = _player.Position with { X = offX + mapW * ts - ts - _player.Width };
                _player.Direction = Direction.Left;
                break;
            case Direction.Right:
                _player.Position = _player.Position with { X = offX + ts };
                _player.Direction = Direction.Right;
                break;
            case Direction.Up:
                _player.Position = _player.Position with { Y = offY + mapH * ts - ts - _player.Height };
                _player.Direction = Direction.Up;
                break;
            case Direction.Down:
                _player.Position = _player.Position with { Y = offY + _player.Height / 2f };
                _player.Direction = Direction.Down;
                break;
        }

        // Lock the new room's doors until the player presses the switch
        foreach (var d in CurrentRoom.Doorways)
            d.IsOpen = false;

        SoundManager.PlaySound("door");
    }

    public void Update(GameTime gameTime)
    {
        if (_shifting)
        {
            _tweens.Update(gameTime);

            // Keep the player animation running during the transition
            _player.Sprite?.Update(gameTime);
        }
        else
        {
            CurrentRoom.Update(gameTime);
        }
    }

    // DepthStencilState that writes 1 into the stencil buffer wherever we draw.
    private static readonly DepthStencilState WriteStencilState = new()
    {
        StencilEnable     = true,
        StencilFunction   = CompareFunction.Always,
        StencilPass       = StencilOperation.Replace,
        ReferenceStencil  = 1,
        DepthBufferEnable = false
    };

    // DepthStencilState that lets pixels through only where stencil == 0 (outside arch areas).
    private static readonly DepthStencilState ReadStencilState = new()
    {
        StencilEnable     = true,
        StencilFunction   = CompareFunction.Equal,
        StencilPass       = StencilOperation.Keep,
        ReferenceStencil  = 0,
        DepthBufferEnable = false
    };

    // BlendState that suppresses all colour output — used while writing the stencil mask.
    private static readonly BlendState StencilOnlyBlend = new()
    {
        ColorWriteChannels = ColorWriteChannels.None
    };

    // Three-pass stencil render:
    //   Pass 1 – rooms drawn normally.
    //   Pass 2 – door-arch rectangles written into stencil buffer (no colour).
    //   Pass 3 – player drawn only where stencil == 0, hiding it inside arch tunnels.
    //
    // All three passes share the same worldTransform (camera + screen scale), so
    // the camera shift is applied uniformly to rooms, arch masks, and player alike.
    public void Render(SpriteBatch spriteBatch, Matrix screenScaleMatrix, Texture2D pixel)
    {
        // Combine camera translation with the screen-scale matrix once.
        // Ordering: camera translates in virtual space first, then scale to screen.
        var worldTransform = _camera.Transform * screenScaleMatrix;

        // Pass 1: rooms and their entities.
        // Each room is drawn in its own Begin/End with the room offset baked into
        // the transform, so rooms and entities just call Draw with no offset knowledge.
        spriteBatch.Begin(transformMatrix: worldTransform, samplerState: SamplerState.PointClamp);
        CurrentRoom.Render(spriteBatch);
        spriteBatch.End();

        if (_nextRoom != null)
        {
            var nextTransform = Matrix.CreateTranslation(_nextRoomOffset.X, _nextRoomOffset.Y, 0) * worldTransform;
            spriteBatch.Begin(transformMatrix: nextTransform, samplerState: SamplerState.PointClamp);
            _nextRoom.Render(spriteBatch);
            spriteBatch.End();
        }

        // Pass 2: write stencil mask at the four door-arch corridors
        spriteBatch.Begin(
            transformMatrix:   worldTransform,
            samplerState:      SamplerState.PointClamp,
            blendState:        StencilOnlyBlend,
            depthStencilState: WriteStencilState);
        DrawArchMasks(spriteBatch, pixel);
        spriteBatch.End();

        // Pass 3: player clipped so it vanishes inside the arch tunnels
        spriteBatch.Begin(
            transformMatrix:   worldTransform,
            samplerState:      SamplerState.PointClamp,
            depthStencilState: ReadStencilState);
        _player.Draw(spriteBatch);
        spriteBatch.End();
    }

    // Draw the four arch-corridor stencil rectangles at their fixed virtual positions.
    // Because worldTransform already contains the camera translation, no manual offset
    // is needed here — the camera shift is applied automatically by SpriteBatch.
    private void DrawArchMasks(SpriteBatch spriteBatch, Texture2D pixel)
    {
        int ts   = GameSettings.TileSize;
        int offX = GameSettings.MapRenderOffsetX;
        int offY = GameSettings.MapRenderOffsetY;
        int mapW = GameSettings.MapWidth;
        int mapH = GameSettings.MapHeight;
        int vh   = GameSettings.VirtualHeight;

        int doorY = (int)(offY + mapH / 2f * ts - ts);
        int doorX = (int)(offX + mapW / 2f * ts - ts);

        int ap = GameSettings.DoorArchPadding;
        spriteBatch.Draw(pixel, new Rectangle(-ts - ap,          doorY,       ts * 2 + ap, ts * 2),          Color.White); // Left
        spriteBatch.Draw(pixel, new Rectangle(offX + mapW * ts,  doorY,       ts * 2 + ap, ts * 2),          Color.White); // Right
        spriteBatch.Draw(pixel, new Rectangle(doorX,             -ts - ap,    ts * 2,      ts * 2 + ap * 2), Color.White); // Top
        spriteBatch.Draw(pixel, new Rectangle(doorX,             vh - ts - ap, ts * 2,     ts * 2 + ap * 2), Color.White); // Bottom
    }
}
