using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/Decision/Look",fileName="Decision_Look_New")]
public class LookDecision : Decision 
{
	private GameObject _player = null;
	private CharacterControls _playerStats = null;

	public override bool Decide(AIStateController pController)
	{
		bool targetVisible = Look(pController);
		return targetVisible;
	}


	private bool Look(AIStateController pController)
	{
		if (_player == null)
			_player = GameObject.FindWithTag ("Player");

		if(_player!=null && _playerStats==null)
			_playerStats = _player.GetComponent<CharacterControls> ();

        if (pController.ChaseTarget != null && pController.ChaseTarget.CompareTag("Enemy"))
        {
            return true;
        }

        if ((_playerStats!=null && _playerStats.IsAlive) && (Vector3.Distance (pController.transform.position, _player.transform.position) < pController.LookRange))
		{
			pController.ChaseTarget = _player.transform;
			return true;
		}


//		RaycastHit hit;
//
//		Debug.DrawRay (pController.Eyes.position, pController.Eyes.forward.normalized * pController.LookRange, Color.green);
//
//		if (Physics.SphereCast (pController.Eyes.position, pController.LookSphere, pController.Eyes.forward, out hit, pController.LookRange) &&
//		    hit.collider.CompareTag ("Player"))
//		{
//			pController.ChaseTarget = hit.transform;
//			return true;
//		} 
//		else
//		{
			return false;
//		}
	}

}
