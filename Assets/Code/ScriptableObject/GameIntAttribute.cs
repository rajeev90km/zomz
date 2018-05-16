using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/GameAttribute/New Integer Attribute",fileName="GA_Int_New")]
public class GameIntAttribute : GameAttribute
{
	public int InitValue;
	public int CurrentValue;

	void OnEnable()
	{
		ResetAttribute ();
	}

	public void ResetAttribute()
	{
		CurrentValue = InitValue;
	}

}


