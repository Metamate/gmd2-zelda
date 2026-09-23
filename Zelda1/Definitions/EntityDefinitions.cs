using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Zelda1.Entities;

namespace Zelda1.Definitions;

public static class EntityDefinitions
{
    private record AnimDef(string Name, int[] Frames, double Interval, bool Loop);

    private static Dictionary<AnimationKey, Animation> _playerAnimations;

    public static void LoadContent(ContentManager content)
    {
        LoadPlayerAnimations(content);
    }

    private static void LoadPlayerAnimations(ContentManager content)
    {
        var root = LoadXml(content, "data/player_animations.xml").Root;

        _playerAnimations = new Dictionary<AnimationKey, Animation>();

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
                _playerAnimations[AnimationKeys.Parse(def.Name)] = atlas.CreateAnimation(def.Frames, def.Interval, def.Loop);
            }
        }
    }

    private static XDocument LoadXml(ContentManager content, string path)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, path));
        return XDocument.Load(stream);
    }

    private static AnimDef ParseAnimDef(XElement e) => new(
        Name:     e.Attribute("name").Value,
        Frames:   e.Attribute("frames").Value.Split(',').Select(int.Parse).ToArray(),
        Interval: double.Parse(e.Attribute("interval").Value, CultureInfo.InvariantCulture),
        Loop:     e.Attribute("loop")?.Value != "false"
    );

    public static Dictionary<AnimationKey, Animation> CreatePlayerAnimations() => _playerAnimations;

}
