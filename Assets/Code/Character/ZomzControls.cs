using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum ZomzAction
{
	NONE = 0,
	MOVE = 1,
	ATTACK = 2
}

public class ZomzActionPoint
{
	public Vector3 Position;
	public ZomzAction ZomzAction;
	public Transform ActionTarget;

	public ZomzActionPoint(Vector3 pPosition, ZomzAction pAction, Transform pTarget)
	{
		Position = pPosition;
		ZomzAction = pAction;
		ActionTarget = pTarget;
	}

}

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
	public int NumZombiesUnderControl
	{
		get { return _zombiesUnderControl.Count; }
	}

	private bool _canUseZomzMode = true;

	private const float ZOMZ_COOLDOWN_TIME = 5f;

	[Header("Debug")]
	[SerializeField]
	private GameObject _debugCanvas;

	[Header("Events")]
	[SerializeField]
	private GameEvent _zomzStartEvent;

	[SerializeField]
	private GameEvent _zomzEndEvent;


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
			Gizmos.DrawWireSphere (transform.position, _characterControls.CharacterStats.ZomzRange);
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
			//TODO check for null and override
			StartCoroutine(ToggleZomzMode ());
		}
	}

	IEnumerator ToggleZomzMode()
	{

		if (_canUseZomzMode)
		{
			_zomzMode = !_zomzMode;

			_debugCanvas.SetActive (_zomzMode);

			if (_zomzMode)
			{
				if (_zomzStartEvent)
					_zomzStartEvent.Raise ();

				_zombiesUnderControl.Clear ();

				_animator.SetTrigger ("idle");

				Collider[] _zombiesHit = Physics.OverlapSphere (transform.position, _characterControls.CharacterStats.ZomzRange, _enemyLayerMask);

				for (int i = 0; i < _zombiesHit.Length; i++)
				{
					AIStateController zCtrl = _zombiesHit [i].GetComponent<AIStateController> ();

					if (zCtrl != null)
					{
						zCtrl.TakeControl ();
						_zombiesUnderControl.Add (zCtrl);
					}
				}
			} 
			else
			{
				_canUseZomzMode = false;

				for (int i = 0; i < _zombiesUnderControl.Count; i++)
				{
					yield return StartCoroutine(_zombiesUnderControl [i].ExecuteActions ());
				}

				for (int i = 0; i < _zombiesUnderControl.Count; i++)
				{
					_zombiesUnderControl [i].RelinquishControl ();
				}

				_zombiesUnderControl.Clear ();

				if (_zomzEndEvent!=null)
					_zomzEndEvent.Raise ();

				StartCoroutine (ZomzCoolDown ());
			}
		}
	}

	IEnumerator ZomzCoolDown()
	{
		yield return new WaitForSeconds (ZOMZ_COOLDOWN_TIME);
		_canUseZomzMode = true;
	}
}
