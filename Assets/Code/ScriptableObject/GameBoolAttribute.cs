using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/GameAttribute/New Bool Attribute", fileName = "GA_Bool_New")]
public class GameBoolAttribute : GameAttribute
{
    public bool InitValue;
    public bool CurrentValue;

    void OnEnable()
    {
        ResetAttribute();
    }

    public void ResetAttribute()
    {
        CurrentValue = InitValue;
    }
}


