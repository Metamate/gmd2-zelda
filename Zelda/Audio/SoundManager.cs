using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Zelda.Audio;

public static class SoundManager
{
    private static Song _music;
    private static readonly Dictionary<string, SoundEffect> _effects = new();

    public static void LoadContent(ContentManager content)
    {
        _music = content.Load<Song>("sounds/music");

        _effects["door"]       = content.Load<SoundEffect>("sounds/door");
        _effects["sword"]      = content.Load<SoundEffect>("sounds/sword");
        _effects["hit-enemy"]  = content.Load<SoundEffect>("sounds/hit_enemy");
        _effects["hit-player"] = content.Load<SoundEffect>("sounds/hit_player");
    }

    public static void PlayMusic()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_music);
    }

    public static void StopMusic() => MediaPlayer.Stop();

    public static void PlaySound(string name)
    {
        if (_effects.TryGetValue(name, out var effect))
            effect.Play();
    }
}
