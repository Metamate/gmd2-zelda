using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.Definitions;

public static class EntityDefinitions
{
    private record AnimDef(string Name, int[] Frames, double Interval, bool Loop);

    public record EnemyStats(int Width, int Height, int WalkSpeed, int Health);

    private static TextureAtlas _enemyAtlas;
    private static Dictionary<string, Animation> _playerAnimations;
    private static Dictionary<string, List<AnimDef>> _enemyDefs;
    private static Dictionary<string, EnemyStats> _enemyStats;

    public static IEnumerable<string> EnemyTypes => _enemyDefs.Keys;

    public static void LoadContent(ContentManager content)
    {
        LoadPlayerAnimations(content);
        LoadEnemyAnimations(content);
    }

    private static void LoadPlayerAnimations(ContentManager content)
    {
        var root = LoadXml(content, "data/player_animations.xml").Root;

        _playerAnimations = new Dictionary<string, Animation>();

        // Each <Atlas> group declares its image and frame size; animations inside
        // are built from that atlas so callers need no knowledge of which image
        // each animation comes from.
        foreach (var atlasEl in root.Elements("Atlas"))
        {
            string image = atlasEl.Attribute("image").Value;
            int frameWidth  = int.Parse(atlasEl.Attribute("frameWidth").Value);
            int frameHeight = int.Parse(atlasEl.Attribute("frameHeight").Value);
            var atlas = TextureAtlas.FromGrid(content.Load<Texture2D>(image), frameWidth, frameHeight);

            foreach (var animEl in atlasEl.Elements("Animation"))
            {
                var def = ParseAnimDef(animEl);
                _playerAnimations[def.Name] = atlas.CreateAnimation(def.Frames, def.Interval, def.Loop);
            }
        }
    }

    private static void LoadEnemyAnimations(ContentManager content)
    {
        var root = LoadXml(content, "data/enemy_animations.xml").Root;

        string image = root.Attribute("atlas").Value;
        int frameWidth  = int.Parse(root.Attribute("frameWidth").Value);
        int frameHeight = int.Parse(root.Attribute("frameHeight").Value);
        _enemyAtlas = TextureAtlas.FromGrid(content.Load<Texture2D>(image), frameWidth, frameHeight);

        var enemyElements = root.Elements("Enemy").ToList();

        _enemyDefs = enemyElements.ToDictionary(
            e => e.Attribute("type").Value,
            e => e.Elements("Animation").Select(ParseAnimDef).ToList()
        );

        _enemyStats = enemyElements.ToDictionary(
            e => e.Attribute("type").Value,
            e => new EnemyStats(
                Width:     int.Parse(e.Attribute("width").Value),
                Height:    int.Parse(e.Attribute("height").Value),
                WalkSpeed: int.Parse(e.Attribute("walkSpeed").Value),
                Health:    int.Parse(e.Attribute("health").Value)
            )
        );
    }

    private static XDocument LoadXml(ContentManager content, string path)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, path));
        return XDocument.Load(stream);
    }

    private static AnimDef ParseAnimDef(XElement e) => new(
        Name:     e.Attribute("name").Value,
        Frames:   e.Attribute("frames").Value.Split(',').Select(int.Parse).ToArray(),
        Interval: double.Parse(e.Attribute("interval").Value),
        Loop:     e.Attribute("loop")?.Value != "false"
    );

    public static Dictionary<string, Animation> CreatePlayerAnimations() => _playerAnimations;

    public static Dictionary<string, Animation> CreateEnemyAnimations(string type) =>
        _enemyDefs[type].ToDictionary(
            d => d.Name,
            d => _enemyAtlas.CreateAnimation(d.Frames, d.Interval, d.Loop)
        );

    public static EnemyStats GetEnemyStats(string type) => _enemyStats[type];
}
