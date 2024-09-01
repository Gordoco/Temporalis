using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static helper class for encoding/decoding AudioClip network communication
/// </summary>
public static class AudioCollection
{
    static Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    /// <summary>
    /// Enters key-value pair for specified audio clip for cross-network reference
    /// </summary>
    /// <param name="clip"></param>
    public static void RegisterAudioClip(AudioClip clip)
    {
        if (clips.ContainsKey(clip.name)) return;
        clips.Add(clip.name, clip);
    }

    /// <summary>
    /// Retrieves a string name for a specified AudioClip for the purpose of network communication
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Retrieves a useable AudioClip from its string identifier for use when recieving network communication
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static AudioClip GetAudioClip(string name)
    {
        if (clips[name]) return clips[name];
        return null;
    }
}
