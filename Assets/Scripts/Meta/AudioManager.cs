using UnityEngine.Audio;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
        }
    }


    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    [Serializable]
    public class Sound
    {
        public string name;

        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume;

        [Range(0.1f, 3f)]
        public float pitch;

        [HideInInspector]
        public AudioSource source;
    }

}
