using System;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda.Audio;
using Zelda.Entities;

namespace Zelda.World;

// Manages the current and next Room during play, including the camera-shift
// transition that mirrors the tween in the Löve2D version.
public class Dungeon
{
    private readonly Player _player;
    private readonly Func<Room> _roomFactory;

    public Room CurrentRoom { get; private set; }
    private Room _nextRoom;

    // Camera translation applied while shifting between rooms
    private Vector2 _camera;
    private Vector2 _cameraTarget;
    private Vector2 _shiftPlayerStart;
    private Vector2 _shiftPlayerEnd;
    private float _shiftProgress;
    private bool _shifting;
    private Direction _shiftDirection;
    private const float ShiftDuration = 1f; // seconds, matches Löve2D Timer.tween(1, ...)

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

        // Camera shifts by one full virtual screen in the transition direction
        _camera = Vector2.Zero;
        _cameraTarget = direction switch
        {
            Direction.Left  => new Vector2(-vw, 0),
            Direction.Right => new Vector2( vw, 0),
            Direction.Up    => new Vector2(0, -vh),
            Direction.Down  => new Vector2(0,  vh),
            _ => Vector2.Zero
        };

        // Position the incoming room offset from the current room
        _nextRoom.AdjacentOffset = _cameraTarget;

        // Store player tween endpoints so they walk through the doorway
        _shiftPlayerStart = _player.Position;
        _shiftPlayerEnd = direction switch
        {
            Direction.Right => new Vector2(vw  + offX + ts,                           _player.Position.Y),
            Direction.Left  => new Vector2(-vw + offX + mapW * ts - ts - _player.Width, _player.Position.Y),
            Direction.Down  => new Vector2(_player.Position.X,                          vh  + offY + _player.Height / 2f),
            Direction.Up    => new Vector2(_player.Position.X,                         -vh  + offY + mapH * ts - ts - _player.Height),
            _ => _player.Position
        };

        _shiftProgress = 0f;
    }

    private void FinishShift()
    {
        _shifting = false;
        _camera = Vector2.Zero;

        CurrentRoom = _nextRoom;
        CurrentRoom.AdjacentOffset = Vector2.Zero;
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
                _player.Position = _player.Position with
                    { X = offX + mapW * ts - ts - _player.Width };
                _player.Direction = Direction.Left;
                break;
            case Direction.Right:
                _player.Position = _player.Position with { X = offX + ts };
                _player.Direction = Direction.Right;
                break;
            case Direction.Up:
                _player.Position = _player.Position with
                    { Y = offY + mapH * ts - ts - _player.Height };
                _player.Direction = Direction.Up;
                break;
            case Direction.Down:
                _player.Position = _player.Position with
                    { Y = offY + _player.Height / 2f };
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
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _shiftProgress = Math.Min(1f, _shiftProgress + dt / ShiftDuration);

            _camera = Vector2.Lerp(Vector2.Zero, _cameraTarget, _shiftProgress);

            // Slide both rooms: current room translates with the camera, next room follows
            _nextRoom.AdjacentOffset = _cameraTarget - _camera;

            // Tween the player through the doorway
            _player.Position = Vector2.Lerp(_shiftPlayerStart, _shiftPlayerEnd, _shiftProgress);

            // Keep the player animation running during the transition
            _player.Sprite?.Update(gameTime);

            if (_shiftProgress >= 1f)
                FinishShift();
        }
        else
        {
            CurrentRoom.Update(gameTime);
        }
    }

    // DepthStencilState that writes 1 into the stencil buffer wherever we draw.
    private static readonly DepthStencilState WriteStencilState = new()
    {
        StencilEnable    = true,
        StencilFunction  = CompareFunction.Always,
        StencilPass      = StencilOperation.Replace,
        ReferenceStencil = 1,
        DepthBufferEnable = false
    };

    // DepthStencilState that lets pixels through only where stencil == 0 (outside arch areas).
    private static readonly DepthStencilState ReadStencilState = new()
    {
        StencilEnable    = true,
        StencilFunction  = CompareFunction.Equal,
        StencilPass      = StencilOperation.Keep,
        ReferenceStencil = 0,
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
    public void Render(SpriteBatch spriteBatch, Matrix transformMatrix, Texture2D pixel)
    {
        // Pass 1: rooms and their entities
        spriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointClamp);
        CurrentRoom.Render(spriteBatch, -_camera);
        if (_nextRoom != null)
            _nextRoom.Render(spriteBatch, _nextRoom.AdjacentOffset);
        spriteBatch.End();

        // Pass 2: write stencil mask at every door-arch corridor
        spriteBatch.Begin(
            transformMatrix:    transformMatrix,
            samplerState:       SamplerState.PointClamp,
            blendState:         StencilOnlyBlend,
            depthStencilState:  WriteStencilState);
        DrawArchMasks(spriteBatch, pixel);
        spriteBatch.End();

        // Pass 3: player, clipped so it vanishes inside the arch tunnels
        spriteBatch.Begin(
            transformMatrix:    transformMatrix,
            samplerState:       SamplerState.PointClamp,
            depthStencilState:  ReadStencilState);
        var savedPos = _player.Position;
        _player.Position -= _camera;
        _player.Draw(spriteBatch);
        _player.Position = savedPos;
        spriteBatch.End();
    }

    // Write the four arch-corridor stencil rectangles, matching the Löve2D Room:render()
    // stencil exactly. In Löve2D a single global translate (–cameraX, –cameraY) is applied
    // before every room render, so both the current and the next room write their stencil
    // rects at identical virtual positions; we replicate that by applying –_camera once here.
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

        // Stencil tracks with the camera — same offset applied to rooms and player.
        int ox = -(int)_camera.X;
        int oy = -(int)_camera.Y;

        // Positions are a direct translation of the Löve2D stencil rectangles.
        spriteBatch.Draw(pixel, new Rectangle(-ts - 6 + ox,           doorY + oy,       ts * 2 + 6, ts * 2),      Color.White); // Left
        spriteBatch.Draw(pixel, new Rectangle(offX + mapW * ts + ox,  doorY + oy,       ts * 2 + 6, ts * 2),      Color.White); // Right
        spriteBatch.Draw(pixel, new Rectangle(doorX + ox,             -ts - 6 + oy,     ts * 2,     ts * 2 + 12), Color.White); // Top
        spriteBatch.Draw(pixel, new Rectangle(doorX + ox,             vh - ts - 6 + oy, ts * 2,     ts * 2 + 12), Color.White); // Bottom
    }
}
