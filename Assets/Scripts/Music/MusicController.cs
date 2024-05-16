using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic()
    {
        if (_audioSource.isPlaying) return;

        _audioSource.Play();
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }
}
