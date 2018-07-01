using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Decision/Attack",fileName="Decision_Attack_New")]
public class AttackDecision : Decision 
{
	private GameObject _player;
	private CharacterControls _playerStats;

	public override bool Decide(AIStateController pController)
	{
		bool targetVisible = CanAttack(pController);

		if (targetVisible)
		{
			pController.Attack ();
		}

		return targetVisible;
	}

	public bool CanAttack(AIStateController pController)
	{
        //if (_player == null)
        //_player = GameObject.FindWithTag ("Player");

        if (pController.ChaseTarget == null)
        {
            return false;
        }
        else if (Vector3.Distance(pController.transform.position, pController.ChaseTarget.position) < pController.CharacterStats.AttackRange)
        {                
            return true;
        }

		//if(_player!=null && _playerStats==null)
		//	_playerStats = _player.GetComponent<CharacterControls> ();

		//if ((_playerStats!=null && _playerStats.IsAlive) && (Vector3.Distance (pController.transform.position, _player.transform.position) < pController.CharacterStats.AttackRange))
		//{
		//	return true;
		//}

		return false;
	}
}
