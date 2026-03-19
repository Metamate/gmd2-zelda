using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Zelda.Definitions;

public static class EntityDefinitions
{
    private record AnimDef(string Name, string Atlas, int[] Frames, double Interval, bool Loop);

    private static List<AnimDef> _playerDefs;
    private static Dictionary<string, List<AnimDef>> _enemyDefs;

    public static void LoadContent(ContentManager content)
    {
        _playerDefs = LoadXml(content, "data/player_animations.xml")
            .Root.Elements("Animation")
            .Select(ParseAnimDef)
            .ToList();

        _enemyDefs = LoadXml(content, "data/enemy_animations.xml")
            .Root.Elements("Enemy")
            .ToDictionary(
                e => e.Attribute("type").Value,
                e => e.Elements("Animation").Select(ParseAnimDef).ToList()
            );
    }

    private static XDocument LoadXml(ContentManager content, string path)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, path));
        return XDocument.Load(stream);
    }

    private static AnimDef ParseAnimDef(XElement e) => new(
        Name:     e.Attribute("name").Value,
        Atlas:    e.Attribute("atlas")?.Value ?? "",
        Frames:   e.Attribute("frames").Value.Split(',').Select(int.Parse).ToArray(),
        Interval: double.Parse(e.Attribute("interval").Value),
        Loop:     e.Attribute("loop")?.Value != "false"
    );

    public static Dictionary<string, Animation> CreatePlayerAnimations(
        TextureAtlas walkAtlas, TextureAtlas swordAtlas)
    {
        var atlases = new Dictionary<string, TextureAtlas>
        {
            ["walk"]  = walkAtlas,
            ["sword"] = swordAtlas
        };

        return _playerDefs.ToDictionary(
            d => d.Name,
            d => atlases[d.Atlas].CreateAnimation(d.Frames, d.Interval, d.Loop)
        );
    }

    public static Dictionary<string, Animation> CreateEnemyAnimations(
        string type, TextureAtlas entityAtlas)
    {
        return _enemyDefs[type].ToDictionary(
            d => d.Name,
            d => entityAtlas.CreateAnimation(d.Frames, d.Interval, d.Loop)
        );
    }
}
