using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private const float DEFAULT_FADE_TIME = 10f;
    [SerializeField] private int DefaultBackground = -1;
    [SerializeField] private float MusicVolume = 1f;
    [SerializeField] private float SFXVolume = 1f;
    [SerializeField] private AudioClip[] clips;

    private int currentBackgroundIndex = -1;
    private float currentVolume;
    private AudioSource source;

    private void Awake()
    {
        DontDestroyOnLoad(this);
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

    public void ChangeBackgroundSound(int newSong, float fadeTime = DEFAULT_FADE_TIME)
    {
        StopAllCoroutines();
        if (currentBackgroundIndex != -1) StartCoroutine(FadeOut(newSong, fadeTime/10));
        else
        {
            currentVolume = 0;
            source.volume = currentVolume;
            SwapTrack(newSong, fadeTime);
        }
    }

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

    public void PlaySoundEffect(AudioClip effect)
    {
        source.PlayOneShot(effect, SFXVolume);
    }
}
