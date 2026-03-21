# Architecture Walkthrough — GMD2 Zelda

A top-down Zelda-like built on **MonoGame/XNA** in C#. This document walks through the architecture from the ground up, pointing at relevant source code as it goes.

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [The Game Loop](#2-the-game-loop)
3. [Game State Machine](#3-game-state-machine)
4. [The Entity Hierarchy](#4-the-entity-hierarchy)
5. [Entity State Machines](#5-entity-state-machines)
6. [The Room and Dungeon](#6-the-room-and-dungeon)
7. [Collision Detection](#7-collision-detection)
8. [Input System](#8-input-system)
9. [Animation and Rendering](#9-animation-and-rendering)
10. [The 3-Pass Stencil Trick](#10-the-3-pass-stencil-trick)
11. [Data-Driven Definitions](#11-data-driven-definitions)
12. [Key Design Decisions](#12-key-design-decisions)

---

## 1. Project Structure

```
gmd2-zelda/
├── GMDCore/               # Reusable engine framework (no game logic)
│   ├── Core.cs            # XNA Game subclass — window, loop, scaling
│   ├── Graphics/          # Sprite, AnimatedSprite, Tilemap, TextureAtlas, …
│   └── Input/             # KeyboardInfo, InputManager
├── Zelda/                 # Game-specific code
│   ├── Game1.cs           # Top-level game; owns the active GameState
│   ├── GameSettings.cs    # All magic numbers in one place
│   ├── Entities/          # IEntity, Entity, Player, Enemy, GameObject
│   ├── States/
│   │   ├── GameStates/    # StartState, PlayState, GameOverState
│   │   ├── EntityStates/  # EntityStateBase, EntityWalkState, EntityIdleState
│   │   └── PlayerStates/  # PlayerWalkState, PlayerIdleState, PlayerSwingSwordState
│   ├── World/             # Room, Dungeon, Doorway
│   ├── Definitions/       # XML loaders for entities and objects
│   ├── Input/             # GameController (keyboard → game actions)
│   ├── Graphics/          # Camera, DebugDraw
│   └── Audio/             # SoundManager
└── Tools/
    └── LabelTiles.cs      # Dev utility — annotates a spritesheet with tile indices
```

The split between `GMDCore` and `Zelda` is intentional: `GMDCore` knows nothing about Zelda. The engine provides window management, a virtual-resolution scaler, input tracking, and sprite/animation primitives. Everything Zelda-specific lives in the `Zelda` project.

### Tooling — `Tools/LabelTiles.cs`

The XML data files reference tiles by their 0-based index into a spritesheet (e.g. `frames="9,10,11,10"` in `enemy_animations.xml`). Knowing which number corresponds to which tile by eye is impractical, so `LabelTiles.cs` is a small standalone script that takes a spritesheet, draws a grid over it with each tile's index number, and saves the result as a new image.

```
dotnet script LabelTiles.cs images/entities.png 16 16
```

It is a pure developer aid — it runs outside the game, produces no build artefacts, and is never referenced by the game code. This is a common pattern in game projects: small throwaway tools that make working with data files practical.

---

## 2. The Game Loop

MonoGame calls `Update` and `Draw` 60 times per second. The game overrides those in two layers.

### Layer 1 — `Core` (`GMDCore/Core.cs`)

```csharp
// Core.cs:41-48
protected override void Update(GameTime gameTime)
{
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
        || Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();

    Input.Update();   // ← snapshot keyboard state for this frame
    base.Update(gameTime);
}
```

`Core` also maintains a **virtual resolution** (384 × 216). On every resize it recalculates `ScreenScaleMatrix` so the game always renders at that resolution regardless of window size (`Core.cs:56-86`).

### Layer 2 — `Game1` (`Zelda/Game1.cs`)

```csharp
// Game1.cs:49-60
protected override void Update(GameTime gameTime)
{
    _currentState?.Update(gameTime);
    base.Update(gameTime);          // calls Core.Update → Input.Update
}

protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);
    _currentState?.Draw(SpriteBatch);
    base.Draw(gameTime);
}
```

`Game1` owns `_currentState` and just delegates to it. The pattern keeps the top-level loop tiny; all real work happens inside states.

**Full call chain per frame:**

```
Core.Update
  └─ Game1.Update
       └─ PlayState.Update
            └─ Dungeon.Update
                 ├─ (if shifting) interpolate camera + player
                 └─ (else) Room.Update
                      ├─ Player.Update  →  PlayerState.Update
                      └─ Enemy.Update   →  ProcessAI + EntityState.Update
```

---

## 3. Game State Machine

`Game1` holds a single `GameStateBase` reference and swaps it via `SetState`:

```csharp
// Game1.cs:42-47
public void SetState(GameStateBase newState)
{
    _currentState?.Exit();
    _currentState = newState;
    _currentState.Enter();
}
```

`Enter` / `Exit` are hooks that let each state initialize and clean up. The three concrete states are:

| State | File | Responsibility |
|---|---|---|
| `StartState` | `States/GameStates/StartState.cs` | Title screen; waits for `GameController.Confirm` |
| `PlayState` | `States/GameStates/PlayState.cs` | Owns `Dungeon`; drives gameplay loop |
| `GameOverState` | `States/GameStates/GameOverState.cs` | Game-over screen |

`PlayState` subscribes to `Dungeon.OnPlayerDied` to know when to switch to `GameOverState`.

---

## 4. The Entity Hierarchy

Everything that lives in a room implements one interface:

```csharp
// Zelda/Entities/IEntity.cs:8-19
public interface IEntity
{
    Vector2 Position { get; set; }
    Rectangle Bounds { get; }
    bool Collidable { get; set; }
    bool IsSolid { get; }
    bool Active { get; set; }

    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
    bool Collides(IEntity other);
}
```

There are three concrete implementations:

### `Entity` (abstract) — `Zelda/Entities/Entity.cs`

Animated living things with health and a state machine.

Key properties:
- `Animations` — a `Dictionary<AnimationKey, Animation>` built at load time
- `Sprite` — an `AnimatedSprite` that plays whichever animation is active
- `State` — the current `EntityStateBase`; changed via `ChangeState`
- `SpriteOffset` — separates the drawn sprite position from the collision box (creates perspective depth)
- `IsInvulnerable`, `GoInvulnerable(duration)` — invincibility frames after taking a hit

`Update` every frame:
```csharp
// Entity.cs:75-92
public virtual void Update(GameTime gameTime)
{
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

    if (IsInvulnerable)
    {
        _invulnerableTimer += dt;
        if (_invulnerableTimer >= _invulnerableDuration)
        {
            IsInvulnerable = false;
            _invulnerableTimer = 0f;
        }
    }

    Sprite?.Update(gameTime);   // advance animation
    State?.Update(gameTime);    // run current state logic
}
```

Notice that `Entity.Update` advances the animation **before** it calls `State.Update`. This means states see the sprite already updated for this frame — they must not advance it a second time.

### `Player` — `Zelda/Entities/Player.cs`

Adds a **Hurtbox**: collision uses only the bottom half of the sprite, which gives a top-down perspective feel (the "feet" collide, not the head).

```csharp
// Player.cs:9-19
public Rectangle Hurtbox
{
    get
    {
        int halfHeight = Height / 2;
        return new Rectangle((int)Position.X, (int)(Position.Y + halfHeight), Width, Height - halfHeight);
    }
}

public override bool Collides(IEntity other) =>
    Collidable && other.Collidable && Hurtbox.Intersects(other.Bounds);
```

### `Enemy` — `Zelda/Entities/Enemy.cs`

Extends `Entity`. Adds `ProcessAI(Room room, GameTime gameTime)` which is called by `Room.Update` before `enemy.Update`. The AI logic is delegated to the current state's `ProcessAI` override.

### `GameObject` — `Zelda/Entities/GameObject.cs`

Static interactive objects (floor switches, chests). No health, no state machine, no AI.

```csharp
// GameObject.cs:9-31
public class GameObject(...) : IEntity
{
    public string Type { get; set; }
    public string State { get; set; }         // e.g. "unpressed" / "pressed"
    private readonly Dictionary<string, int> _stateFrames;

    public event Action OnCollide;
    public void Collide() => OnCollide?.Invoke();
    // ...
}
```

Behaviour is wired externally via the `OnCollide` event rather than through subclasses, because objects are loaded from XML and a subclass per object type would defeat that purpose (see the comment at `Room.cs:121-122`).

---

## 5. Entity State Machines

The **state machine** is the backbone of all entity behaviour.

### `EntityStateBase` — `Zelda/States/EntityStates/EntityStateBase.cs`

```csharp
public abstract class EntityStateBase
{
    protected Entity Entity { get; }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(GameTime gameTime) { }
    public virtual void ProcessAI(Room room, GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch) { Entity.DrawSprite(spriteBatch); }
}
```

States hold a reference to their owner entity. Transitions are always triggered from inside a state by calling `Entity.ChangeState(new SomeOtherState(...))`.

### `EntityWalkState` — `Zelda/States/EntityStates/EntityWalkState.cs`

Used by both enemies and (as a base) the player. `Update` moves the entity in its current direction and wall-clamps it, setting `Bumped = true` if it hit a wall.

```csharp
// EntityWalkState.cs:39
Entity.Position += Entity.Direction.ToVector2() * Entity.WalkSpeed * dt;
```

`ProcessAI` uses the room's shared `Random` to pick a new direction when the entity bumps a wall or its movement timer expires. It can also spontaneously transition to `EntityIdleState`.

### Player States

Player states extend or parallel the entity states but read from `GameController` instead of running AI.

**`PlayerWalkState`** (`Zelda/States/PlayerStates/PlayerWalkState.cs`):
1. Reads input and updates `_player.Direction`.
2. Calls `base.Update(gameTime)` — the inherited `EntityWalkState.Update` — to move and wall-clamp.
3. If `Bumped`, probes one step ahead for an open doorway; if found, calls `_dungeon.BeginShift`.

```csharp
// PlayerWalkState.cs:46-48
base.Update(gameTime);  // move + wall collision

if (Bumped)
    CheckDoorwayTransition(gameTime);
```

**`PlayerSwingSwordState`** (`Zelda/States/PlayerStates/PlayerSwingSwordState.cs`):
- `Enter` builds a `_swordHitbox` rectangle in front of the player based on facing direction.
- `Update` checks each enemy's `Bounds` against `_swordHitbox` every frame.
- The animation is non-looping; when `_player.Sprite.TimesPlayed > 0` the swing is over and the state transitions back to `PlayerIdleState`.

```csharp
// PlayerSwingSwordState.cs:57-63
if (_player.Sprite != null && _player.Sprite.TimesPlayed > 0)
{
    _player.Sprite.TimesPlayed = 0;
    _player.ChangeState(new PlayerIdleState(_player, _dungeon));
    return;
}
```

**State diagram (player):**

```
PlayerIdleState  ──(move key)──►  PlayerWalkState  ──(space)──►  PlayerSwingSwordState
      ▲                                  │                                │
      └──────────(no move key)───────────┘          (animation done)─────┘
```

---

## 6. The Room and Dungeon

### `Room` — `Zelda/World/Room.cs`

A `Room` is generated procedurally in its constructor:

1. **`GenerateWallsAndFloors`** (lines 37-62) — fills a `Tilemap` using tile IDs from `GameSettings`, randomly selecting wall variants.
2. **`GenerateEntities`** (lines 79-103) — spawns `GameSettings.RoomEnemyCount` enemies at random positions. Types and stats come from `EntityDefinitions`.
3. **`GenerateObjects`** (lines 106-134) — places a floor switch and wires its `OnCollide` event inline.
4. **`GenerateDoorways`** (lines 137-143) — creates four `Doorway` instances (one per direction), all initially closed.

The switch behaviour is wired right in the generator:
```csharp
// Room.cs:123-132
switchObj.OnCollide += () =>
{
    if (switchObj.State == "unpressed")
    {
        switchObj.State = "pressed";
        foreach (var d in Doorways)
            d.IsOpen = true;
        SoundManager.PlaySound("door");
    }
};
```

### `Dungeon` — `Zelda/World/Dungeon.cs`

Manages the active room and the **room-transition animation**.

#### How a shift is triggered

The chain starts in `PlayerWalkState`. Each frame, if the player bumped a wall, `CheckDoorwayTransition` probes one step ahead, checks whether a doorway at that position is open, and if so calls `_dungeon.BeginShift(_player.Direction)` (`PlayerWalkState.cs:51-77`).

#### `BeginShift` — setting up the transition

`BeginShift` (lines 53-99) sets up everything needed for the animation in one shot:

1. Creates the next `Room` via the factory delegate and opens all its doorways (so the player can walk in from any side).
2. Computes `_shiftTarget` — the final camera position. If moving right, the camera must travel one full virtual-screen width to the right:
   ```csharp
   _shiftTarget = direction switch
   {
       Direction.Left  => new Vector2(-vw, 0),
       Direction.Right => new Vector2( vw, 0),
       Direction.Up    => new Vector2(0, -vh),
       Direction.Down  => new Vector2(0,  vh),
   };
   ```
3. Places the next room at `_nextRoomOffset = _shiftTarget`. The next room never moves in world space — the camera lerp is what makes it slide into view.
4. Stores `_shiftPlayerStart` (current position) and `_shiftPlayerEnd` (the matching entry point in the next room, one screen away), so the player appears to walk through the doorway.

The spatial layout for a rightward shift looks like this:

```
 world space (before shift)
┌─────────────────┐  ┌─────────────────┐
│                 │  │                 │
│  CurrentRoom    │  │   _nextRoom     │
│                 │  │  (at x = +vw)   │
└─────────────────┘  └─────────────────┘
 camera at (0,0) ──────────────────────► camera lerps to (vw, 0)
```

#### `Update` during a shift

While `_shifting` is true, `Room.Update` is **not called** — gameplay is frozen. Instead, `Dungeon.Update` only advances the animation:

```csharp
// Dungeon.cs:147-161
_shiftProgress = Math.Min(1f, _shiftProgress + dt / GameSettings.RoomShiftDuration);
_camera.Position = Vector2.Lerp(Vector2.Zero, _shiftTarget, _shiftProgress);
_player.Position = Vector2.Lerp(_shiftPlayerStart, _shiftPlayerEnd, _shiftProgress);
_player.Sprite?.Update(gameTime);   // keep walk animation running
```

Because both rooms and the player are drawn through the same `camera.Transform * screenScaleMatrix`, the entire scene slides uniformly — no per-object offset arithmetic needed.

#### `FinishShift` — landing in the new room

When `_shiftProgress` reaches `1`, `FinishShift` (lines 101-141):
1. Promotes `_nextRoom` to `CurrentRoom`.
2. Snaps the player's position to the correct entry point inside the new room (the lerp endpoint lands just outside the wall; the snap places them just inside it).
3. Locks all of the new room's doors — the player must find and press the switch to open them again.

---

## 7. Collision Detection

Collision is **not** handled in a central system. Each interaction type lives where it makes logical sense:

| Interaction | Where handled | Why |
|---|---|---|
| Player ↔ Enemy | `Room.Update` (lines 167-176) | Needs room-level state to trigger `OnPlayerDied` |
| Player ↔ GameObject | `Room.Update` (lines 179-185) | Needs to reach doorways list to open doors |
| Sword ↔ Enemy | `PlayerSwingSwordState.Update` (lines 47-55) | Tightly coupled to sword animation frame |
| Entity ↔ Wall | `EntityWalkState.Update` (lines 41-75) | Wall-clamp is part of movement |

The comment in `Room.cs:145-148` explains this explicitly:
> Collision is handled here rather than in a separate system because each interaction type has different consequences (damage, doors, death) that require access to room-level state. Sword–enemy collision lives in PlayerSwingSwordState because it is tightly coupled to the swing animation.

Player–enemy collision uses `Player.Collides(enemy)` which internally uses `Hurtbox`, while sword–enemy collision checks `_swordHitbox.Intersects(enemy.Bounds)` directly, giving the sword its own independent reach rectangle.

---

## 8. Input System

### `KeyboardInfo` — `GMDCore/Input/KeyboardInfo.cs`

Stores two keyboard snapshots every frame:

```csharp
public void Update()
{
    PreviousState = CurrentState;
    CurrentState  = Keyboard.GetState();
}

public bool WasKeyJustPressed(Keys key) =>
    CurrentState.IsKeyDown(key) && PreviousState.IsKeyUp(key);
```

`WasKeyJustPressed` fires for exactly **one frame** (the transition from up to down). This is how sword-swing avoids auto-firing while held.

### `GameController` — `Zelda/Input/GameController.cs`

A static class that translates raw keys into named game actions:

```csharp
public static bool SwingSword => Core.Input.Keyboard.WasKeyJustPressed(Keys.Space);
public static bool Left       => Core.Input.Keyboard.IsKeyDown(Keys.Left)
                              || Core.Input.Keyboard.IsKeyDown(Keys.A);
// ...
```

States query `GameController`, not `KeyboardInfo` directly. If the key bindings ever change, only `GameController` needs updating.

---

## 9. Animation and Rendering

### The Graphics Stack

```
TextureAtlas  ─── splits a spritesheet into ──► TextureRegion[]
Animation     ─── holds a list of ─────────────► TextureRegion  +  delay + loop flag
AnimatedSprite ─── plays an ───────────────────► Animation
Entity.Sprite  ─── is an ──────────────────────► AnimatedSprite
```

### `AnimatedSprite` — `GMDCore/Graphics/AnimatedSprite.cs`

```csharp
// AnimatedSprite.cs:47-73
public void Update(GameTime gameTime)
{
    _elapsed += gameTime.ElapsedGameTime;

    if (_elapsed >= _animation.Delay)
    {
        _elapsed -= _animation.Delay;
        _currentFrame++;

        if (_currentFrame >= _animation.Frames.Count)
        {
            if (_animation.Loop)
                _currentFrame = 0;
            else
            {
                _currentFrame = _animation.Frames.Count - 1;
                TimesPlayed++;       // ← used by PlayerSwingSwordState to detect end
            }
        }

        Region = _animation.Frames[_currentFrame];
    }
}
```

For non-looping animations (sword swing), the sprite holds on the last frame and increments `TimesPlayed`. The state machine polls `TimesPlayed` to know the animation is done.

### Invulnerability Flash

```csharp
// Entity.cs:94-101
public void DrawSprite(SpriteBatch spriteBatch)
{
    bool flash = IsInvulnerable &&
                 (_invulnerableTimer / GameSettings.InvulFlashInterval) % 2 < 1;

    Color drawColor = flash ? Color.White * GameSettings.InvulFlashAlpha : Color.White;
    Sprite?.Region?.Draw(spriteBatch, Position - SpriteOffset, drawColor);
}
```

The expression `(timer / interval) % 2 < 1` alternates between 0 and 1 twice per interval, producing a regular blink without any extra state.

### Camera and Screen Scaling

`Camera` (`Zelda/Graphics/Camera.cs`) is minimal — just a position and a translation matrix:
```csharp
public Matrix Transform => Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
```

The combined transform passed to every `SpriteBatch.Begin` is:
```csharp
var worldTransform = _camera.Transform * screenScaleMatrix;
```

Camera translation happens in virtual-pixel space first, then everything is scaled to the real screen.

---

## 10. The 3-Pass Stencil Trick

The doorway arches create an illusion: the player sprite visually "disappears into" the tunnel as they walk through. This is achieved with a stencil buffer in three draw passes (`Dungeon.Render`, lines 202-239).

**Pass 1** — Render rooms and all entities normally.

**Pass 2** — Draw invisible rectangles at the four door-arch positions. These write `1` into the stencil buffer but produce **no colour output** (`StencilOnlyBlend` suppresses colour writes). The arch area is now "marked" in the stencil.

**Pass 3** — Redraw the player, but only where `stencil == 0` (outside the arch). Inside the arch the player is clipped out.

```csharp
// Dungeon.cs:223-238
// Pass 2: write stencil mask
spriteBatch.Begin(
    transformMatrix:   worldTransform,
    blendState:        StencilOnlyBlend,       // no colour output
    depthStencilState: WriteStencilState);     // stencil ← 1
DrawArchMasks(spriteBatch, pixel);
spriteBatch.End();

// Pass 3: player clipped outside stencil == 1
spriteBatch.Begin(
    transformMatrix:   worldTransform,
    depthStencilState: ReadStencilState);      // pass only where stencil == 0
_player.Draw(spriteBatch);
spriteBatch.End();
```

All three passes use the same `worldTransform`, so the arch mask automatically follows the camera during room transitions.

---

## 11. Data-Driven Definitions

The rule in this codebase is: **content lives in XML, behaviour lives in C#**. Three loaders in `Zelda/Definitions/` enforce this.

### Enemies — `EntityDefinitions` + `enemy_animations.xml`

Each enemy type is one `<Enemy>` block that declares its stats and its full set of directional animations as frame-index lists:

```xml
<!-- Zelda/Content/data/enemy_animations.xml -->
<EnemyAnimations atlas="images/entities" frameWidth="16" frameHeight="16">
  <Enemy type="skeleton" width="16" height="16" walkSpeed="20" health="1">
    <Animation name="walk-down"  frames="9,10,11,10"  interval="0.2" />
    <Animation name="idle-down"  frames="10"          interval="0.2" />
    <!-- ... one entry per direction × state -->
  </Enemy>
  <Enemy type="slime" ...>
    ...
  </Enemy>
</EnemyAnimations>
```

At startup, `EntityDefinitions.LoadContent` parses this once into two dictionaries keyed by type string: one for stats (`EnemyStats` records) and one for animation definitions. When `Room.GenerateEntities` spawns an enemy it calls:

```csharp
// EntityDefinitions.cs:99-103
public static Dictionary<AnimationKey, Animation> CreateEnemyAnimations(string type) =>
    _enemyDefs[type].ToDictionary(
        d => AnimationKeys.Parse(d.Name),
        d => _enemyAtlas.CreateAnimation(d.Frames, d.Interval, d.Loop)
    );
```

Each call produces a fresh `Dictionary<AnimationKey, Animation>` for that enemy instance. The C# enemy code has no hardcoded frame numbers — it only knows animation *keys* like `AnimationKey.WalkDown`. Adding a new enemy type is an XML edit plus a spritesheet row; no C# changes required.

### Interactive Objects — `GameObjectDefinitions` + `object_definitions.xml`

Objects are even simpler. Each `<Object>` maps named states to frame indices:

```xml
<!-- Zelda/Content/data/object_definitions.xml -->
<Object type="switch" atlas="images/switches" frameWidth="16" frameHeight="18"
        width="16" height="16" defaultState="unpressed">
  <State name="unpressed" frame="1" />
  <State name="pressed"   frame="0" />
</Object>
```

`GameObject.Draw` uses this at runtime:
```csharp
// GameObject.cs:45-49
public void Draw(SpriteBatch spriteBatch)
{
    int frame = _stateFrames[State];   // State is "unpressed" or "pressed"
    _atlas.GetRegion($"frame_{frame}").Draw(spriteBatch, Position, Color.White);
}
```

The object renders whatever frame the XML says corresponds to its current state string. It has no `if` branches per type — the data drives both the visuals and the valid state names.

### Door Tiles — `Doorway` + `door_layouts.xml`

Each doorway is a 2×2 composite of tiles. The XML maps direction × open/closed to exactly four tile IDs plus offsets in tile units:

```xml
<!-- Zelda/Content/data/door_layouts.xml -->
<Layout state="open" direction="left">
  <Tile id="180" dx="-1" dy="0" />
  <Tile id="181" dx="0"  dy="0" />
  <Tile id="199" dx="-1" dy="1" />
  <Tile id="200" dx="0"  dy="1" />
</Layout>
```

`Doorway.Draw` picks the right layout dict and renders each tile relative to the doorway's position (`Doorway.cs:105-109`). Changing door art means updating the tile IDs in XML; the draw code never changes.

---

## 12. Key Design Decisions

### State machines everywhere
Both the game and its entities use the same `Enter / Update / Exit` pattern. The approach keeps each behaviour self-contained: a state knows its entity, reads input or room state, and decides when to hand off to the next state.

### Inheritance vs. composition for player states
`PlayerWalkState` **inherits** `EntityWalkState` to reuse the movement and wall-collision logic, then adds input handling and doorway detection on top. This avoids duplicating the wall-clamp math while keeping the player-specific logic separate.

### Decentralised collision
There is no `CollisionSystem` class. Each collision site is handled where it has natural access to the needed context (see section 7). This trades a central overview for locality — you look at a state or a room method and see exactly what that interaction does.

### Event-driven decoupling

Events are used to prevent lower-level classes from knowing about higher-level ones.

**`OnPlayerDied` — bubbling up the call stack**

When the player runs out of health, the consequence (transitioning to `GameOverState`) lives in `PlayState`. But the detection happens deep inside `Room.Update`. Rather than giving `Room` a reference to `PlayState`, the event bubbles up one layer at a time:

```
Room.OnPlayerDied  →  Dungeon.OnPlayerDied  →  PlayState handler
```

Each layer simply forwards the event to its own subscribers:
```csharp
// Dungeon.cs:49
room.OnPlayerDied += () => OnPlayerDied?.Invoke();
```

`Room` knows nothing about `Dungeon`. `Dungeon` knows nothing about `PlayState`. Each class only references the thing directly below it.

**`GameObject.OnCollide` — wiring behaviour without subclasses**

`GameObject` is fully data-driven: its type, visuals, and states all come from XML. If it also defined behaviour through subclasses (a `SwitchGameObject`, a `ChestGameObject`, etc.), the data-driven design would collapse — you'd need a new class for every object type.

Instead, `GameObject` exposes a single event:
```csharp
// GameObject.cs:29-31
public event Action OnCollide;
public void Collide() => OnCollide?.Invoke();
```

The *caller* — whoever creates the object and knows the game rules — subscribes to it with whatever logic is appropriate. In `Room.GenerateObjects`, the switch behaviour is wired inline:

```csharp
// Room.cs:123-132
switchObj.OnCollide += () =>
{
    if (switchObj.State == "unpressed")
    {
        switchObj.State = "pressed";
        foreach (var d in Doorways)
            d.IsOpen = true;
        SoundManager.PlaySound("door");
    }
};
```

`GameObject` itself has no `if (type == "switch")` branch anywhere. It just fires the event; what happens is entirely up to the subscriber. A new object type with completely different behaviour requires only an XML entry and a new `OnCollide` subscription — the `GameObject` class stays unchanged.

### Single source of truth for constants
`GameSettings.cs` holds every magic number: tile sizes, walk speeds, enemy counts, flash intervals, etc. States and rooms reference it by name rather than embedding literals.
