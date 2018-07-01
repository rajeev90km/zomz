using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private GameData _gameData;

    [SerializeField]
    private AudioData _audioData;

    [SerializeField]
    private AudioClip _levelBGM;

    //[SerializeField]
    //private UnityEditor.SceneAsset _nextLevel;

    [Header("Interstitials")]
    [SerializeField]
    private Conversation _levelStartInterstitial;

    [SerializeField]
    private Conversation _levelEndInterstitial;

    [SerializeField]
    private Conversation _levelLostInterstitial;

    [Header("Events")]
    [SerializeField]
    private GameEvent _conversationStartEvent;

    [SerializeField]
    private GameEvent _conversationEndEvent;

    [SerializeField]
    private GameEvent _levelStartEvent;


    void Start()
    {
        OnLevelStart();
    }

    public void OnLevelStart()
    {
        StartConversation(_levelStartInterstitial);
    }

    public void OnLevelEnd()
    {
        Debug.Log("Level End");
        StartConversation(_levelEndInterstitial);
    }

    public void OnLevelLost()
    {
        Debug.Log("Level Lost");
        StartConversation(_levelLostInterstitial);
    }

    void StartConversation (Conversation pConversation)
    {
        _gameData.CurrentConversation.Conversation = pConversation;
        _conversationStartEvent.Raise();
    }

	public void EndConversation()
	{
        Debug.Log(_gameData.CurrentConversation.Conversation);

        if (_gameData.CurrentConversation.Conversation == _levelStartInterstitial)
            _levelStartEvent.Raise();

        if (_gameData.CurrentConversation.Conversation == _levelEndInterstitial)
        {
            SceneManager.LoadScene(2);
        }

        if (_gameData.CurrentConversation.Conversation == _levelLostInterstitial)
            SceneManager.LoadScene(1);

        _audioData.CurrentPlayingBGM = _levelBGM;

        _gameData.CurrentConversation.Conversation = null;
        TogglePauseLevel(false);
	}

	void TogglePauseLevel(bool pEnable)
    {
        _gameData.IsPaused = pEnable;
    }
}
