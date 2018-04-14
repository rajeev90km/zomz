using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Action/Patrol",fileName="Action_Patrol_New")]
public class PatrolAction : Action 
{
	public override void Act(AIStateController pController)
	{
		Patrol (pController);
	}

	private void Patrol(AIStateController pController)
	{
		pController.navMeshAgent.destination = pController.wayPoints [pController.NextWayPoint].position;
		pController.navMeshAgent.Resume ();

		if (pController.navMeshAgent.remainingDistance <= pController.navMeshAgent.stoppingDistance && !pController.navMeshAgent.pathPending)
		{
			pController.NextWayPoint = Random.Range (0, pController.wayPoints.Count);
		}
	}
}
