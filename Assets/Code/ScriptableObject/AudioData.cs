using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Data/New AudioData", fileName = "AudioData_New")]
public class AudioData : ScriptableObject 
{
    public AudioClip CurrentPlayingBGM;

	private void OnEnable()
	{
        CurrentPlayingBGM = null;
	}
}
