using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Action/Attack",fileName="Action_Attack_New")]
public class AttackAction : Action 
{
	public override void Act(AIStateController pController)
	{
		Attack (pController);
	}

	private void Attack(AIStateController pController)
	{
		pController.navMeshAgent.Stop ();
		Debug.Log ("Attacking");
	}	
}
