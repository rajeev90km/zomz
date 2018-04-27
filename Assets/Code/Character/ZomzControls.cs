using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
public class ZomzControls : MonoBehaviour {

	private bool _zomzMode = false;
	public bool ZomzMode
	{
		get{ return _zomzMode;}	
	}

	private int _enemyLayerMask;
	private CharacterControls _characterControls;
	private Animator _animator;

	private List<AIStateController> _zombiesUnderControl;

	void Start () 
	{
		_animator = GetComponent<Animator> ();
		_zombiesUnderControl = new List<AIStateController> ();
		_characterControls = GetComponent<CharacterControls> ();
		_enemyLayerMask = (1 << LayerMask.NameToLayer ("Enemy"));
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
		RaycastHit hit; 
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		if (_zomzMode)
		{
			if (Input.GetMouseButtonDown (0))
			{
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, _enemyLayerMask))
				{
					if (hit.transform != null)
					{
						for (int i = 0; i < _zombiesUnderControl.Count; i++)
						{
							if (hit.collider.gameObject != _zombiesUnderControl [i].gameObject)
								_zombiesUnderControl [i].ClearCurrentControl ();
							else
								_zombiesUnderControl [i].SelectCurrentForControl ();
						}

					}
				}
			}
		}

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
			_animator.SetTrigger ("idle");

			Collider[] _zombiesHit = Physics.OverlapSphere (transform.position, _characterControls.CharacterStats.AttackRange, _enemyLayerMask);

			for (int i = 0; i < _zombiesHit.Length; i++)
			{
				AIStateController zCtrl =  _zombiesHit [i].GetComponent<AIStateController>();

				if (zCtrl != null)
				{
					zCtrl.TakeControl();
					_zombiesUnderControl.Add (zCtrl);
				}
			}

		} 
		else
		{
			for (int i = 0; i < _zombiesUnderControl.Count; i++)
			{
				_zombiesUnderControl [i].RelinquishControl ();
			}
			_zombiesUnderControl.Clear ();
		}
	}
}
