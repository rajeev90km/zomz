using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Decision/ActiveStateDecision",fileName="Decision_ActiveState_New")]
public class ActiveStateDecision : Decision 
{
	GameObject _player;

	void OnEnable()
	{
		_player = GameObject.FindWithTag ("Player");
	}

	public override bool Decide(AIStateController pController)
	{
		if (pController.ChaseTarget == null)
			pController.ChaseTarget = _player.transform;

		bool chaseTargetIsActive = pController.ChaseTarget.gameObject.activeSelf;
		return chaseTargetIsActive;
	}
}
