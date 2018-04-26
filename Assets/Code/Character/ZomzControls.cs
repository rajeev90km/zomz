using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
public class ZomzControls : MonoBehaviour {

	private bool _zomzMode = false;
	private int _enemyLayerMask;
	private CharacterControls _characterControls;

	private List<AIStateController> _zombiesUnderControl;

	void Start () 
	{
		_zombiesUnderControl = new List<AIStateController> ();
		_characterControls = GetComponent<CharacterControls> ();
		_enemyLayerMask |= (1 << LayerMask.NameToLayer ("Enemy"));
	}

	void OnDrawGizmos()
	{
		if (_characterControls != null)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere (transform.position, _characterControls.CharacterStats.AttackRange);
		}
	}

	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.Z))
		{
			ToggleZomzMode ();	
		}
	}

	void ToggleZomzMode()
	{
		_zomzMode = !_zomzMode;

		if (_zomzMode)
		{
			Collider[] _zombiesHit = Physics.OverlapSphere (transform.position, _characterControls.CharacterStats.AttackRange, _enemyLayerMask);

			for (int i = 0; i < _zombiesHit.Length; i++)
			{
				AIStateController zCtrl =  _zombiesHit [i].GetComponent<AIStateController>();

				if (zCtrl != null)
				{
					zCtrl.BeingControlled = true;
					_zombiesUnderControl.Add (zCtrl);
				}
			}

		} 
		else
		{
			for (int i = 0; i < _zombiesUnderControl.Count; i++)
			{
				_zombiesUnderControl [i].BeingControlled = false;
			}
			_zombiesUnderControl.Clear ();
		}
	}
}
