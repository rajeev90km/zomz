using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Decision/Attack",fileName="Decision_Attack_New")]
public class AttackDecision : Decision 
{
	public override bool Decide(AIStateController pController)
	{
		bool targetVisible = CanAttack(pController);
		return targetVisible;
	}

	public bool CanAttack(AIStateController pController)
	{
		GameObject _player = GameObject.FindWithTag ("Player");

		if (Vector3.Distance (pController.transform.position, _player.transform.position) < pController.AttackRange)
		{
			return true;
		}

		return false;
	}
}
