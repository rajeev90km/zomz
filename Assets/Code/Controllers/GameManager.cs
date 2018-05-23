using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private GameData _gameData;

    [Header("Conversations")]
    [SerializeField]
    private Conversation _gameStartConversation;

    [Header("Events")]
    [SerializeField]
    private GameEvent _conversationStartEvent;

    [SerializeField]
    private GameEvent _conversationEndEvent;

	
	void Start () 
    {
        //Game Start Conversation
        _gameData.CurrentConversation.Conversation = _gameStartConversation;
        _conversationStartEvent.Raise();

	}

    public void TogglePauseGame(bool pEnable)
    {
        _gameData.IsPaused = pEnable;
    }
	
	
	void Update () 
    {
		
	}
}
