/// <summary>
/// Entry point for the content builder. When run, it builds the game's raw assets according
/// to the rules in GetContentCollection() below.
/// </summary>
/// <remarks>
/// Game projects run this automatically when they build (see BuildContent.targets).
/// For more details, see the MonoGame documentation:
///
///    https://docs.monogame.net/articles/getting_started/content_pipeline/content_builder_project.html
/// </remarks>

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Framework.Content.Pipeline.Builder;

var builder = new Builder();

// The build arguments (platform, source and output folders) are passed in by the game project.
if (args.Length > 0)
    builder.Run(args);
else
    builder.Run(new ContentBuilderParams { Mode = ContentBuilderMode.None });

return builder.FailedToBuild > 0 ? -1 : 0;

public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var content = new ContentCollection();

        // Images are built into textures. Mipmaps aren't needed for 2D pixel art.
        content.Include<WildcardRule>("*.png", new TextureImporter(), new TextureProcessor
        {
            ColorKeyEnabled = false,
            GenerateMipmaps = false,
            PremultiplyAlpha = true,
        });

        // A .spritefont describes a font (e.g. a .ttf next to it), a size and a set of characters.
        // The builder renders those characters into a texture.
        content.Include<WildcardRule>("*.spritefont", new FontDescriptionImporter(), new FontDescriptionProcessor
        {
            PremultiplyAlpha = true,
            TextureFormat = TextureProcessorOutputFormat.Compressed,
        });

        // Short sounds become sound effects, loaded fully into memory.
        content.Include<WildcardRule>("*.wav", new WavImporter(), new SoundEffectProcessor
        {
            Quality = ConversionQuality.Best,
        });

        // Some .wav files are music, not sound effects. Rules added later win, so these
        // override the *.wav rule above.
        content.Include("sounds/music.wav", new WavImporter(), new SongProcessor { Quality = ConversionQuality.Best });

        // The XML data files are read by our own code at runtime, so they are copied as they are.
        content.IncludeCopy<WildcardRule>("*.xml");

        return content;
    }
}
