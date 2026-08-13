using UnityEngine;
using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[Serializable]
public struct MusicHandlerData
{
    public AudioClip musicClip;
    [Range(0f, 1f)]
    public float volume;
}

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private MusicHandlerData[] _mainTracks;
    [SerializeField] private MusicHandlerData _menuTrack;
    [SerializeField] private MusicHandlerData _lostTrack;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float  _secondsBeforeChange;

    private bool _isLost;
    private int _levelIndex;
    private float SecondsBeforeChange => _audioSource?.time ?? 10 - 5f;

    public static MusicHandler Instance;
    private MusicHandlerData _nextTrack;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += (scene, mode) => OnLevelLoaded(scene);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnLevelLoaded(Scene level)
    {
        _levelIndex = level.buildIndex;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerObj.GetComponent<CarHandler>().OnPLayerCrashed += OnPlayerLost;
        }
        PlayTrackWithFade();
    }

    private void PlayTrackWithFade()
    {
        CalculateNextTrack();
        Timing.RunCoroutine(_ChangeTrackWithFadeCoroutine(_nextTrack.musicClip, 5f));
    }

    private void CalculateNextTrack()
    {
        if (_isLost)
        {
            _nextTrack = _lostTrack;
            return;
        }
        if (_levelIndex == 0)
        {
            _nextTrack = _menuTrack;
            return;
        }
        else
        {
            int trackIndex = UnityEngine.Random.Range(0, _mainTracks.Length);
            _nextTrack = _mainTracks[trackIndex];
            return;
        }
    }

    private void OnPlayerLost(CarHandler handler)
    {
        Debug.Log("Player Lost MUSIC");
        _isLost = true;
        _ChangeTrackWithFadeCoroutine(_lostTrack.musicClip, 3);
    }

    private IEnumerator<float> _ChangeTrackWithFadeCoroutine(AudioClip clip, float fadeDuration = 5f)
    {
        float percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime / fadeDuration;
            _audioSource.volume = Mathf.Lerp(1f, 0f, percent);
            yield return Timing.WaitForOneFrame;
        }
        _audioSource.clip = clip;
        _audioSource.Play();
        percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime / fadeDuration;
            _audioSource.volume = Mathf.Lerp(0f, 1f, percent);
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> _ChangeTrackCoroutine()
    {
        int trackIndex = UnityEngine.Random.Range(0, _mainTracks.Length);
        float curLength = _mainTracks[trackIndex].musicClip.length;
        yield return Timing.WaitForSeconds(curLength-_secondsBeforeChange);
        Timing.RunCoroutine(_ChangeTrackWithFadeCoroutine(_mainTracks[trackIndex].musicClip));
    }
    private void Update()
    {
        while (_audioSource.time < (_audioSource.clip?.length ?? 60) - SecondsBeforeChange) 
        {
            return;
        }
        PlayTrackWithFade();
    }
}
