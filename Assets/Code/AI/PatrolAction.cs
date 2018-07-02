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
        if (pController.IsAlive)
        {
            if (pController.navMeshAgent.isActiveAndEnabled)
            {

                pController.navMeshAgent.destination = pController.wayPoints[pController.NextWayPoint].position;
                pController.navMeshAgent.isStopped = false;

                if (pController.navMeshAgent.remainingDistance <= 1f)
                {
                    pController.NextWayPoint = Random.Range(0, pController.wayPoints.Count);
                }
            }
        }
	}
}
