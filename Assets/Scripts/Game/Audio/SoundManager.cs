using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : NetworkBehaviour
{
    [SerializeField] private const float DEFAULT_FADE_TIME = 10f;
    [SerializeField] private int DefaultBackground = -1;
    [SerializeField] private float MusicVolume = 1f;
    [SerializeField] private float SFXVolume = 1f;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private bool bCarryThroughLevels = false;

    private int currentBackgroundIndex = -1;
    private float currentVolume;
    private AudioSource source;

    private void Awake()
    {
        if (bCarryThroughLevels) DontDestroyOnLoad(this);
        source = GetComponent<AudioSource>();
        source.loop = true;
    }

    private void Start()
    {
        if (DefaultBackground != -1)
        {
            ChangeBackgroundSound(DefaultBackground);
        }
        GameObject AudioManager = GameObject.FindGameObjectWithTag("AudioManager");
        SoundManager masterManager = AudioManager.GetComponent<SoundManager>();
        SFXVolume = masterManager.SFXVolume;
        MusicVolume = masterManager.MusicVolume;
    }

    /// <summary>
    /// Changes the background music to the specified "clips" index of the SoundManager
    /// </summary>
    /// <param name="newSong"></param>
    /// <param name="fadeTime"></param>
    public void ChangeBackgroundSound(int newSong, float fadeTime = DEFAULT_FADE_TIME)
    {
        if (isClient && !isServer)
        {
            //OnClient
            Server_ChangeBackgroundForEveryone(newSong, fadeTime);
        }
        else if (isServer)
        {
            //OnServer
            Client_ChangeBackground(newSong, fadeTime);
        }
        else
        {
            //Offline
            ChangeBackground_Functionality(newSong, fadeTime);
        }
    }

    [Command]
    private void Server_ChangeBackgroundForEveryone(int newSong, float fadeTime)
    {
        Client_ChangeBackground(newSong, fadeTime);
    }

    [ClientRpc]
    private void Client_ChangeBackground(int newSong, float fadeTime)
    {
        ChangeBackground_Functionality(newSong, fadeTime);
    }

    private void ChangeBackground_Functionality(int newSong, float fadeTime)
    {
        StopAllCoroutines();

        if (currentBackgroundIndex != -1) StartCoroutine(FadeOut(newSong, fadeTime / 10));
        else
        {
            currentVolume = 0;
            source.volume = currentVolume;
            SwapTrack(newSong, fadeTime);
        }
    }

    /// <summary>
    /// Smoothly fades out main background music track
    /// </summary>
    /// <param name="newSong"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator FadeOut(int newSong, float time)
    {
        currentVolume = MusicVolume;
        float count = 0;
        while (count < time)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            count += Time.deltaTime;
            currentVolume -= Time.deltaTime * (MusicVolume / time);
            if (currentVolume < source.volume) source.volume = currentVolume;
        }
        SwapTrack(newSong, time*10);
    }

    /// <summary>
    /// Swaps main music tracks using a FadeIn timer
    /// </summary>
    /// <param name="newSong"></param>
    /// <param name="fadeTime"></param>
    private void SwapTrack(int newSong, float fadeTime)
    {
        source.Pause();
        currentBackgroundIndex = newSong;
        source.clip = clips[currentBackgroundIndex];
        source.Play();
        StartCoroutine(FadeIn(fadeTime));
    }

    private IEnumerator FadeIn(float time)
    {
        float count = 0;
        while (count < time)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            count += Time.deltaTime;
            currentVolume += Time.deltaTime * (MusicVolume/time);
            source.volume = currentVolume;
        }
    }

    [Server]
    public void PlaySoundEffect(AudioClip effect)
    {
        int clipID = AudioCollection.GetClipID(effect);
        if (clipID == -1)
        {
            Debug.LogError("ERROR: [SoundManager.cs - Attempting to play sound effect with unregistered clip number]");
            return;
        }
        PlaySoundEffect(clipID);
    }

    [Server]
    public void PlaySoundEffect(int effectID)
    {
        if (isClient && !isServer)
        {
            Server_PlaySoundForEveryone(effectID);
        }
        else if (isServer)
        {
            Client_PlaySoundEffect(effectID);
        }
    }

    [Command]
    private void Server_PlaySoundForEveryone(int effectID)
    {
        Client_PlaySoundEffect(effectID);
    }

    [ClientRpc]
    private void Client_PlaySoundEffect(int effectID)
    {
        PlayEffect(effectID);
    }

    private void PlayEffect(int effectID)
    {
        AudioClip effect = AudioCollection.GetAudioClip(effectID);
        if (!effect) return;
        source.PlayOneShot(effect, SFXVolume);
    }
}
