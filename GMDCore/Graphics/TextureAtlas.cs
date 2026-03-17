using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

public class TextureAtlas
{
    private Dictionary<string, TextureRegion> _regions;
    private Dictionary<string, Animation> _animations;
    public Texture2D Texture { get; set; }

    public TextureAtlas()
    {
        _regions = [];
        _animations = [];
    }

    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = [];
        _animations = [];
    }

    public void AddRegion(string name, int x, int y, int width, int height)
    {
        TextureRegion region = new(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    public void Clear()
    {
        _regions.Clear();
    }

    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        TextureAtlas atlas = new();

        string filePath = Path.Combine(content.RootDirectory, fileName);

        using Stream stream = TitleContainer.OpenStream(filePath);
        using XmlReader reader = XmlReader.Create(stream);
        XDocument doc = XDocument.Load(reader);
        XElement root = doc.Root;

        // The <Texture> element contains the content path for the Texture2D to load.
        // So we will retrieve that value then use the content manager to load the texture.
        string texturePath = root.Element("Texture").Value;
        atlas.Texture = content.Load<Texture2D>(texturePath);

        // The <Regions> element contains individual <Region> elements, each one describing
        // a different texture region within the atlas.  
        //
        // Example:
        // <Regions>
        //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
        //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
        // </Regions>
        //
        // So we retrieve all of the <Region> elements then loop through each one
        // and generate a new TextureRegion instance from it and add it to this atlas.
        var regions = root.Element("Regions")?.Elements("Region");

        if (regions != null)
        {
            foreach (var region in regions)
            {
                string name = region.Attribute("name")?.Value;
                int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                if (!string.IsNullOrEmpty(name))
                {
                    atlas.AddRegion(name, x, y, width, height);
                }
            }
        }

        // The <Animations> element contains individual <Animation> elements, each one describing
        // a different animation within the atlas.
        //
        // Example:
        // <Animations>
        //      <Animation name="animation" delay="100">
        //          <Frame region="spriteOne" />
        //          <Frame region="spriteTwo" />
        //      </Animation>
        // </Animations>
        //
        // So we retrieve all of the <Animation> elements then loop through each one
        // and generate a new Animation instance from it and add it to this atlas.
        var animationElements = root.Element("Animations").Elements("Animation");

        if (animationElements != null)
        {
            foreach (var animationElement in animationElements)
            {
                string name = animationElement.Attribute("name")?.Value;
                float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");
                TimeSpan delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

                List<TextureRegion> frames = [];

                var frameElements = animationElement.Elements("Frame");

                if (frameElements != null)
                {
                    foreach (var frameElement in frameElements)
                    {
                        string regionName = frameElement.Attribute("region").Value;
                        TextureRegion region = atlas.GetRegion(regionName);
                        frames.Add(region);
                    }
                }

                Animation animation = new(frames, delay);
                atlas.AddAnimation(name, animation);
            }
        }

        return atlas;
    }

    public Sprite CreateSprite(string regionName)
    {
        TextureRegion region = GetRegion(regionName);
        return new Sprite(region);
    }

    public void AddAnimation(string animationName, Animation animation)
    {
        _animations.Add(animationName, animation);
    }

    public Animation GetAnimation(string animationName)
    {
        return _animations[animationName];
    }

    public bool RemoveAnimation(string animationName)
    {
        return _animations.Remove(animationName);
    }

    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        Animation animation = GetAnimation(animationName);
        return new AnimatedSprite(animation);
    }
}
