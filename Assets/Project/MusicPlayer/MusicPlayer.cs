using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer _instance;

    public static MusicPlayer Instance
    {
        get
        {
            return _instance;
        }
    }

    public bool IsReady
    {
        get
        {
            return _isLoadingDone && _currentClip != null && _currentClip.loadState == AudioDataLoadState.Loaded;
        }
    }

    public bool IsLoadingDone
    {
        get
        {
            return _isLoadingDone;
        }
    }

    private AudioSource _audioSource;
    private AudioClip _currentClip;
    private bool _isLoadingDone;

    public void Awake()
    {
        _instance = this;
        _currentClip = null;

        _audioSource = gameObject.GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Load (string filePath)
    {
        _currentClip = null;

        StartCoroutine(LoadFileCoroutine(filePath));
    }

    IEnumerator LoadFileCoroutine(string filePath)
    {
        _isLoadingDone = false;

        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.OGGVORBIS))
        {
            yield return uwr.SendWebRequest();

            try
            {
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            catch (Exception err)
            {
                Debug.LogError(err.Message);
            }
        }

        _currentClip = clip;
        _isLoadingDone = true;
    }

    public void Play ()
    {
        if (_currentClip != null)
        {
            if (_audioSource.clip != null)
            {
                _audioSource.clip.UnloadAudioData();
            }

            _audioSource.clip = _currentClip;
            _audioSource.Play();
        }
    }
}