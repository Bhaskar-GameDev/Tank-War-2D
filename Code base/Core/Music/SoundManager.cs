using UnityEngine;
using System;
using Unity.Netcode;
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
            Intialize();
        }
        else
        {
            Destroy(gameObject);
            StopMusic();
        }
    }
    private void Intialize()
    {
        audioSource.Play();
    }
    private void StopMusic()
    {
        audioSource.Stop();
    }
}