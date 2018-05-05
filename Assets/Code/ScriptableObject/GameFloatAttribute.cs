using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/GameAttribute/New Float Attribute",fileName="GA_Float_New")]
public class GameFloatAttribute : GameAttribute
{
	public float InitValue;
	public float CurrentValue;

	void OnEnable()
	{
		CurrentValue = InitValue;
	}
}


