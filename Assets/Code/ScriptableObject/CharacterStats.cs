using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/Data/New Character Stats",fileName="CS_New")]
public class CharacterStats : ScriptableObject 
{
	public float Health = 100;

	public float WalkSpeed;

	public float RunSpeed;

	public float AttackRate = 1.2f;

	public float AttackStrength = 10f;

}
