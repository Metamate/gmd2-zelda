using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Zelda.Definitions;

public static class GameObjectDefinitions
{
    public record ObjectStats(int Width, int Height, string DefaultState, Dictionary<string, int> StateFrames);

    private static Dictionary<string, ObjectStats> _stats;

    public static void LoadContent(ContentManager content)
    {
        using var stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, "data/object_definitions.xml"));
        var doc = XDocument.Load(stream);

        _stats = doc.Root.Elements("Object").ToDictionary(
            e => e.Attribute("type").Value,
            e => new ObjectStats(
                Width:        int.Parse(e.Attribute("width").Value),
                Height:       int.Parse(e.Attribute("height").Value),
                DefaultState: e.Attribute("defaultState").Value,
                StateFrames:  e.Elements("State").ToDictionary(
                    s => s.Attribute("name").Value,
                    s => int.Parse(s.Attribute("frame").Value)
                )
            )
        );
    }

    public static ObjectStats GetStats(string type) => _stats[type];
}
