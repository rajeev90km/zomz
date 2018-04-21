using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Decision/Attack",fileName="Decision_Attack_New")]
public class AttackDecision : Decision 
{
	[NonSerialized]
	public float period = float.MaxValue;

	private GameObject _player;
	private CharacterControls _playerControls;

	public override bool Decide(AIStateController pController)
	{
		bool targetVisible = CanAttack(pController);

		if (targetVisible)
		{
			Attack (pController);
		}

		return targetVisible;
	}

	public void Attack(AIStateController pController)
	{
		if (pController.CharacterStats)
		{
			if (period > pController.CharacterStats.AttackRate)
			{
				pController.Animator.SetTrigger ("attack");

				_playerControls = _player.GetComponent<CharacterControls> ();

				if(_playerControls)
					_playerControls.Hurt (pController.CharacterStats.AttackStrength);

				period = 0;
			}

			period += Time.deltaTime;
		}
	}

	public bool CanAttack(AIStateController pController)
	{
		_player = GameObject.FindWithTag ("Player");

		if (Vector3.Distance (pController.transform.position, _player.transform.position) < pController.AttackRange)
		{
			return true;
		}

		return false;
	}
}
