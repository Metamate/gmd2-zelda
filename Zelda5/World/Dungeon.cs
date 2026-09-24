using System;
using GMDCore.Tweening;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Zelda5.Entities;
using Zelda5.Graphics;

namespace Zelda5.World;

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

    // The rooms and the player are all drawn through the camera, so the camera shift
    // moves everything together. While walking through a door, the player is drawn on
    // top of the door arch; the next step fixes that with the stencil buffer.
    public void Render(SpriteBatch spriteBatch, Matrix screenScaleMatrix)
    {
        // Combine camera translation with the screen-scale matrix once.
        // Ordering: camera translates in virtual space first, then scale to screen.
        var worldTransform = _camera.Transform * screenScaleMatrix;

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

        spriteBatch.Begin(transformMatrix: worldTransform, samplerState: SamplerState.PointClamp);
        _player.Draw(spriteBatch);
        spriteBatch.End();
    }
}
