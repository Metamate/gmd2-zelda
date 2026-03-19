using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Zelda.Definitions;

public static class GameObjectDefinitions
{
    public record ObjectStats(int Width, int Height, string DefaultState, Dictionary<string, int> StateFrames, TextureAtlas Atlas);

    private static Dictionary<string, ObjectStats> _stats;

    public static void LoadContent(ContentManager content)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, "data/object_definitions.xml"));
        var doc = XDocument.Load(stream);

        _stats = doc.Root.Elements("Object").ToDictionary(
            e => e.Attribute("type").Value,
            e =>
            {
                string image    = e.Attribute("atlas").Value;
                int frameWidth  = int.Parse(e.Attribute("frameWidth").Value);
                int frameHeight = int.Parse(e.Attribute("frameHeight").Value);
                var atlas = TextureAtlas.FromGrid(content.Load<Texture2D>(image), frameWidth, frameHeight);

                return new ObjectStats(
                    Width:        int.Parse(e.Attribute("width").Value),
                    Height:       int.Parse(e.Attribute("height").Value),
                    DefaultState: e.Attribute("defaultState").Value,
                    StateFrames:  e.Elements("State").ToDictionary(
                        s => s.Attribute("name").Value,
                        s => int.Parse(s.Attribute("frame").Value)
                    ),
                    Atlas: atlas
                );
            }
        );
    }

    public static ObjectStats GetStats(string type) => _stats[type];
}
