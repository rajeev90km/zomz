using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    [SerializeField]
    private GameData _gameData;

    [SerializeField]
    private UnityEditor.SceneAsset _nextLevel;

    [Header("Interstitials")]
    [SerializeField]
    private Conversation _levelStartInterstitial;

    [SerializeField]
    private Conversation _levelEndInterstitial;

    [Header("Events")]
    [SerializeField]
    private GameEvent _conversationStartEvent;

    [SerializeField]
    private GameEvent _conversationEndEvent;

    [SerializeField]
    private GameEvent _levelStartEvent;

	
	void Start () 
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

    void StartConversation (Conversation pConversation)
    {
        _gameData.CurrentConversation.Conversation = pConversation;
        _conversationStartEvent.Raise();
    }

	public void EndConversation()
	{
        if (_gameData.CurrentConversation.Conversation == _levelStartInterstitial)
            _levelStartEvent.Raise();

        if (_gameData.CurrentConversation.Conversation == _levelEndInterstitial)
            SceneManager.LoadScene(_nextLevel.ToString());

        _gameData.CurrentConversation.Conversation = null;
        TogglePauseLevel(false);
	}

	void TogglePauseLevel(bool pEnable)
    {
        _gameData.IsPaused = pEnable;
    }
}
