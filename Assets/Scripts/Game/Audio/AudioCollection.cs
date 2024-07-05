using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioCollection
{
    static List<AudioClip> clips = new List<AudioClip>();

    public static void RegisterAudioClip(AudioClip clip)
    {
        if (clips.Contains(clip)) return;
        clips.Add(clip);
    }

    public static int GetClipID(AudioClip clip)
    {
        if (clips.Contains(clip)) return clips.IndexOf(clip);
        return -1;
    }

    public static AudioClip GetAudioClip(int index)
    {
        if (index >= 0 && index < clips.Count)
        {
            return clips[index];
        }
        return null;
    }
}
