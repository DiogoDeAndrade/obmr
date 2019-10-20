using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
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

    void _PlaySound(AudioClip sound, float volume = 1.0f, float pitch = 1.0f)
    {
        foreach (var audioSource in audioSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sound;
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.Play();
                return;
            }
        }

        GameObject newGameObject = new GameObject();
        newGameObject.transform.parent = transform;
        newGameObject.name = "AudioSource";
        var snd = newGameObject.AddComponent<AudioSource>();
        snd.clip = sound;
        snd.volume = volume;
        snd.pitch = pitch;
        snd.Play();

        audioSources.Add(snd);
    }

    public static void PlaySound(AudioClip sound, float volume = 1.0f, float pitch = 1.0f)
    {
        if (instance == null) return;

        instance._PlaySound(sound, volume, pitch);
    }
}
