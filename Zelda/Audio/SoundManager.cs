using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Zelda.Audio;

public static class SoundManager
{
    private static Song _music;

    public static void LoadContent(ContentManager content)
    {
        _music = content.Load<Song>("sounds/music");
    }

    public static void PlayMusic()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_music);
    }

    public static void StopMusic() => MediaPlayer.Stop();
}
