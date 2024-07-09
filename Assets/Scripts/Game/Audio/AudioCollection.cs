using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioCollection
{
    static Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    public static void RegisterAudioClip(AudioClip clip)
    {
        if (clips.ContainsKey(clip.name)) return;
        clips.Add(clip.name, clip);
    }

    public static string GetClipName(AudioClip clip)
    {
        if (clips.ContainsValue(clip))
        {
            foreach (KeyValuePair<string, AudioClip> pair in clips)
            {
                if (pair.Value == clip) return pair.Key;
            }
        }
        return null;
    }

    public static AudioClip GetAudioClip(string name)
    {
        if (clips[name]) return clips[name];
        return null;
    }
}
