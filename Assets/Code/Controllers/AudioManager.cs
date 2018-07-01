using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

    [SerializeField]
    private AudioData _audioData;

    private AudioSource _aSource;

    private const float AUDIO_FADE_TIME = 2f;

    private Coroutine _fadeToNextCouroutine;

	void Start () 
    {
        _aSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (_audioData.CurrentPlayingBGM != null)
        {
            if (_audioData.CurrentPlayingBGM != _aSource.clip)
            {
                if(_fadeToNextCouroutine == null)
                    _fadeToNextCouroutine = StartCoroutine(FadeToNextClip());
            }
        }
	}

    IEnumerator FadeToNextClip()
    {
        float time = 0f;

        if (_aSource.clip != null)
        {
            //Fade out
            while (time < 1)
            {
                _aSource.volume = Mathf.Lerp(1, 0, time);
                time += Time.deltaTime / AUDIO_FADE_TIME;
                yield return null;
            }
        }

        time = 0f;

        _aSource.clip = _audioData.CurrentPlayingBGM;
        _aSource.Play();

        //Fade In
        while (time < 1)
        {
            _aSource.volume = Mathf.Lerp(0, 1, time);
            time += Time.deltaTime / AUDIO_FADE_TIME;
            yield return null;
        }

        _aSource.volume = 1;


        StopCoroutine(_fadeToNextCouroutine);
        _fadeToNextCouroutine = null;

        yield return null;
    }
}
