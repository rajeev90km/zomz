using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Decision/Look",fileName="Decision_Look_New")]
public class LookDecision : Decision 
{
	public override bool Decide(AIStateController pController)
	{
		bool targetVisible = Look(pController);
		return targetVisible;
	}


	private bool Look(AIStateController pController)
	{
		RaycastHit hit;

		Debug.DrawRay (pController.Eyes.position, pController.Eyes.forward.normalized * pController.LookRange, Color.green);

		if (Physics.SphereCast (pController.Eyes.position, pController.LookSphere, pController.Eyes.forward, out hit, pController.LookRange) &&
		    hit.collider.CompareTag ("Player"))
		{
			pController.ChaseTarget = hit.transform;
			return true;
		} 
		else
		{
			return false;
		}
	}

}
