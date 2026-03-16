using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Soundmanager build after https://www.youtube.com/watch?v=g5WT91Sn3hg

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    [SerializeField] private AudioMixer _mixer;

    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _ambientSource;

    [SerializeField] private GlobalAudioLibrary _library;
    public GlobalAudioLibrary Library => _library;

    private Coroutine _ambientRoutine;
    private Coroutine _musicRoutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("SoundManger is NULL");

            return _instance;
        }
    }

    public void PauseBackgroundSounds()
    {
        _musicSource.Pause();
        _ambientSource.Pause();
    }

    public void UnPauseBackgroundSounds()
    {
        _musicSource.UnPause();
        _ambientSource.UnPause();
    }

    // Music
    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        if (_musicRoutine != null)
            StopCoroutine(_musicRoutine);

        _musicRoutine = StartCoroutine(FadeIn(_musicSource, clip, fadeDuration));
    }

    public void FadeOutMusic(float duration = 1f)
    {
        if (_musicRoutine != null)
            StopCoroutine(_musicRoutine);

        _musicRoutine = StartCoroutine(FadeOut(_musicSource, duration));
    }

    // Ambient
    public void PlayAmbient(AudioClip clip, float fadeDuration = 1f)
    {
        if (_ambientRoutine != null)
            StopCoroutine(_ambientRoutine);

        _ambientRoutine = StartCoroutine(FadeIn(_ambientSource, clip, fadeDuration));
    }

    public void FadeOutAmbient(float duration = 1f)
    {
        if (_ambientRoutine != null)
            StopCoroutine(_ambientRoutine);

        _ambientRoutine = StartCoroutine(FadeOut(_ambientSource, duration));
    }

    // Transitions and Fades
    private IEnumerator FadeIn(AudioSource source, AudioClip clip, float duration)
    {
        source.clip = clip;
        source.volume = 0f;
        source.Play();

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        source.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        source.Stop();
        source.volume = 1f;
    }

    // SFX
    public static void PlaySound(SFXEntry entry, float volume = 1)
    {
        AudioClip[] clips = entry.Clips;
        if (clips == null || clips.Length == 0)
            return;

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        _instance._sfxSource.PlayOneShot(randomClip, volume);
    }

    // Volumes
    public void SetMasterVolume(float value)
    {
        _mixer.SetFloat("MasterVolume", LinearToDb(value));
    }

    public void SetMusicVolume(float value)
    {
        _mixer.SetFloat("MusicVolume", LinearToDb(value));
    }

    public void SetSFXVolume(float value)
    {
        _mixer.SetFloat("SFXVolume", LinearToDb(value));
    }

    public void SetAmbientVolume(float value)
    {
        _mixer.SetFloat("AmbientVolume", LinearToDb(value));
    }

    private float LinearToDb(float value)
    {
        if (value <= 0.0001f)
            return -80f; // Silence

        return Mathf.Log10(value) * 20f;
    }
}
