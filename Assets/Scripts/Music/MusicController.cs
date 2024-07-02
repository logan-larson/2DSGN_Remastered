using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private BoolVariable _themeMusicObjectExists;

    private void Awake()
    {
        if (_themeMusicObjectExists.Value)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _themeMusicObjectExists.Value = true;
        }
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
