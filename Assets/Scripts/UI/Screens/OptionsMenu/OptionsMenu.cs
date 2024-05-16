using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private AudioMixer _musicMixer;

    [SerializeField]
    private AudioMixer _sfxMixer;

    void Start()
    {
        _canvas.gameObject.SetActive(false);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxMixer.SetFloat("Volume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        _musicMixer.SetFloat("Volume", volume);
    }
}
