using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public enum SoundType { SoundFX };

    public AudioMixerGroup mixerSoundFX;
        
    List<AudioSource> audioSources;

    public static SoundManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        audioSources = new List<AudioSource>();

        GetComponentsInChildren<AudioSource>(true, audioSources);
    }

    AudioSource _PlaySound(SoundType type, AudioClip sound, float volume = 1.0f, float pitch = 1.0f)
    {
        foreach (var audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sound;
                audioSource.outputAudioMixerGroup = GetMixer(type);
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.Play();
                return audioSource;
            }
        }

        GameObject newGameObject = new GameObject();
        newGameObject.transform.parent = transform;
        newGameObject.name = "AudioSource";
        var snd = newGameObject.AddComponent<AudioSource>();
        snd.clip = sound;
        snd.outputAudioMixerGroup = GetMixer(type);
        snd.volume = volume;
        snd.pitch = pitch;
        snd.Play();

        audioSources.Add(snd);

        return snd;
    }

    AudioMixerGroup GetMixer(SoundType type)
    {
        switch (type)
        {
            case SoundType.SoundFX:
                return mixerSoundFX;
            default:
                break;
        }

        return null;
    }

    public static AudioSource PlaySound(SoundType type, AudioClip sound, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return null;

        return instance._PlaySound(type, sound, volume, pitch);
    }
}
