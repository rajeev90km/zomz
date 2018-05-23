using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/GameAttribute/New Current Conversation Attribute", fileName = "GA_CurrentConversation")]
public class CurrentConversation : ScriptableObject 
{
    public Conversation Conversation;

    public void ResetSelection()
    {
        Conversation = null;
    }

    private void OnEnable()
    {
        ResetSelection();
    }

    private void OnDisable()
    {
        ResetSelection();
    }
}
