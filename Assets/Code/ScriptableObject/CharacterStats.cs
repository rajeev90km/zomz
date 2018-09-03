using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/Data/New Character Stats",fileName="CS_New")]
public class CharacterStats : ScriptableObject 
{
	public float Health = 100;

	public float WalkSpeed;

	public float RunSpeed;

    public float PushSpeed = 1f;

    public float LookRange = 4f;

	public float AttackRate = 1.2f;

    public float HurtRate = 1f;

	public float AttackStrength = 10f;

	public float AttackRange = 1.2f;

	public float ZomzRange = 12f;

	public float ZomzSpeed = 5f;

    public float AttackDamageToSelf = 0f;

    public float DiveDistance = 3f;

    public float DiveTime = 1f;

    public float StompRange = 5;

    public float ExplosionRange = 7f;

}
