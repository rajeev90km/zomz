using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public enum CharacterPosition
{
    LEFT1 = 0,
    LEFT2 = 1,
    LEFT3 = 2,
    RIGHT1 = 3,
    RIGHT2 = 4,
    RIGHT3 = 5,
}

[Serializable]
public class ConversationEntity
{
    public string CharacterName;

    public string Text;

    public Sprite Avatar;

    public CharacterPosition CharPosition;

    public Sprite BackgroundImage;

    public AudioClip DialogueVO;

    public AudioClip BackgroundMusic;

    public AudioClip SFX;

    public AudioClip ExitSFX;
}

/// <summary>
/// Contains back and forth conversation inside a list with parameters that can be tweaked
/// </summary>
[CreateAssetMenu(menuName = "Zomz/Data/New Conversation", fileName = "Assets/Resources/Data/Conversations/CNV_New")]
public class Conversation : ScriptableObject 
{
    public List<ConversationEntity> AllDialogues = new List<ConversationEntity>();

    public bool PauseGamePlay = true;
}
