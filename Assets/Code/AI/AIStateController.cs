using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class AIStateController : MonoBehaviour {

	private bool _isAIOn = true;

	private bool _beingControlled = false;
	public bool BeingControlled
	{
		get{ return _beingControlled; }	 
		set{ _beingControlled = value;}
	}

	private bool _selectedForControl = false;
	public bool IsSelectedForControl
	{
		get { return _selectedForControl; }
	}

	[SerializeField]
	private CharacterStats _characterStats;
	public CharacterStats CharacterStats
	{
		get { return _characterStats; }
	}

	private bool _isAlive = true;
	public bool IsAlive
	{
		get { return _isAlive; }
	}

	public float _currentHealth;

	[SerializeField]
	private Transform _eyes;
	public Transform Eyes
	{
		get { return _eyes; }
		set { _eyes = value; }
	}

	[Header("Models")]
	[SerializeField]
	private GameObject _normalModeModel;
	[SerializeField]
	private GameObject _zomzModeModel;

	[SerializeField]
	private float _lookRange = 10f;
	public float LookRange
	{
		get{ return _lookRange; }
		set{ _lookRange = value; }
	}

	[SerializeField]
	private float _lookSphere = 10f;
	public float LookSphere
	{
		get{ return _lookSphere; }
		set{ _lookSphere = value; }
	}

	[SerializeField]
	private float _attackRange = 1f;
	public float AttackRange
	{
		get{ return _attackRange; }
		set{ _attackRange = value; }
	}

	[SerializeField]
	private float _attackRate = 1f;
	public float AttackRate
	{
		get{ return _attackRate; }
		set{ _attackRate = value; }
	}

	[Header("AI States")]
	[SerializeField]
	private State _initState;
	public State InitState
	{
		get{ return _initState; }
		set{ InitState = value; }
	}

	[SerializeField]
	private State _currentState;
	public State CurrentState
	{
		get{ return _currentState; }
		set{ _currentState = value; }
	}

	[SerializeField]
	private State _remainState;
	public State RemainState
	{
		get{ return _remainState; }
		set{ _remainState = value; }
	}

	[SerializeField]
	private State _deadState;
	public State DeadState
	{
		get{ return _deadState; }
		set{ _deadState = value; }
	}

	[Header("FX")]
	[SerializeField]
	private GameObject _hurtFX;

	private Animator _animator;
	public Animator Animator
	{
		get{ return _animator;}
	}

	[HideInInspector]
	public NavMeshAgent navMeshAgent;

	[HideInInspector]
	public Transform ChaseTarget;

	[HideInInspector]
	public float StateTimeElapsed = 0f;

	[SerializeField]
	private GameObject _wayPointsObj;

	[HideInInspector]
	public List<Transform> wayPoints;

	private float period = float.MaxValue;

	private CharacterControls _playerControls;
	private GameObject _player;

	private bool _zomzAttack = false;

	private int _nextWayPoint;
	public int NextWayPoint
	{
		get{ return _nextWayPoint; }
		set{ _nextWayPoint = value; }
	}

	private LineRenderer _lineRenderer;
	private List<Vector3> points = new List<Vector3> ();

	private int _groundLayerMask;
	private int _enemyPlayerMask;
	private ZomzActionSystem _zactionSystem;

	void Start () 
	{
		_zactionSystem = _zomzModeModel.GetComponent<ZomzActionSystem> ();	

		_groundLayerMask |= (1 << LayerMask.NameToLayer ("Ground"));
		_enemyPlayerMask = (1 << LayerMask.NameToLayer ("Enemy")) | (1 << LayerMask.NameToLayer ("Player")); 

		_lineRenderer = GetComponent<LineRenderer> ();
		_currentState = _initState;
		_currentHealth = _characterStats.Health;
		_player = GameObject.FindWithTag ("Player");
		_playerControls = _player.GetComponent<CharacterControls> ();

		//Get all waypoints
		if (_wayPointsObj != null)
		{
			for(int i=0;i<_wayPointsObj.transform.childCount;i++)
			{
				wayPoints.Add(_wayPointsObj.transform.GetChild(i));
			}
		}

		//Get Navmesh Agent
		navMeshAgent = GetComponent<NavMeshAgent>();

		//Set Animator
		_animator = GetComponent<Animator>();
		_animator.SetTrigger (_currentState.AnimationTrigger);
	}

	public void SelectCurrentForControl()
	{
		_selectedForControl = true;
	}

	public void ClearCurrentControl()
	{
		_selectedForControl = false;
	}

	public void TakeControl()
	{
		_beingControlled = true;
		points.Clear ();
		points.Add (transform.position);
	}

	public void RelinquishControl()
	{
		_beingControlled = false;
		_zomzModeModel.transform.localPosition = Vector3.zero;
		_zomzModeModel.transform.localRotation = Quaternion.identity;
	}

	void Update () 
	{ 
		if (_isAlive)
		{
			if(_isAIOn)
				CurrentState.UpdateState (this);


			//ZOMZ
			if(Input.GetKeyDown(KeyCode.Z))
				ToggleAI();


			if (_beingControlled && _selectedForControl)
			{
				_zomzModeModel.SetActive (true);
				_zactionSystem.IsSelected = true;

				if (DistanceToLastPoint (_zomzModeModel.transform.position) > 0.25f)
				{
					points.Add (_zomzModeModel.transform.position);
					_lineRenderer.positionCount = points.Count;
					_lineRenderer.SetPositions (points.ToArray ());
				}

				//Show attack sphere
				if (Input.GetKeyDown (KeyCode.Alpha1))
				{
					_zomzAttack = true;
				}

				if (_zomzAttack)
				{
					if (Input.GetMouseButtonDown (1))
					{
						RaycastHit hit; 
						Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
						if (Physics.Raycast (ray, out hit, Mathf.Infinity, _enemyPlayerMask))
						{
							if (hit.transform != null)
							{
								if (hit.collider.gameObject != this.gameObject)
								{
									if (Vector3.Distance (_zomzModeModel.transform.position, hit.transform.position) <= _characterStats.AttackRange)
									{
										//add action to queue
										Debug.Log (hit.transform.name);
										_zactionSystem.Animator.SetTrigger ("attack");
									}
								}
							}
						}
					}
				}
			}

			if (!_beingControlled)
			{
				_zomzModeModel.SetActive (false);
				_zactionSystem.IsSelected = false;
				points.Clear ();
				_lineRenderer.positionCount = points.Count;
				_lineRenderer.SetPositions (points.ToArray ());
				_selectedForControl = false;
				_zomzAttack = false;
			}

		}
	}

	private float DistanceToLastPoint(Vector3 pPoint)
	{
		if (!points.Any())
			return float.MaxValue;
		return Vector3.Distance (points.Last (), pPoint);
	}

	void ResetAI()
	{
		navMeshAgent.isStopped = true;
		_animator.SetTrigger ("idle");
	}

	public void ToggleAI()
	{
		_isAIOn = !_isAIOn;	

		if (!_isAIOn)
		{
			ResetAI ();
		}
		else
		{
			TransitionToState (InitState);
		}

	}

	void OnDrawGizmos()
	{
		if (_currentState != null)
		{
			Gizmos.color = _currentState.SceneGizmoColor;
			Gizmos.DrawWireSphere (transform.position, 2);
		}
	}

	public void TakeDamage(float pDamage)
	{
		StartCoroutine (DamageCoroutine (pDamage));
	}

	IEnumerator DamageCoroutine(float pDamage)
	{
		if (_isAlive)
		{
			if (_currentHealth - pDamage > 0)
				_currentHealth -= pDamage;
			else
				_currentHealth = 0;

			if (_currentHealth > 0)
			{
				yield return new WaitForSeconds(0.7f);

				if (_hurtFX != null)
					Instantiate (_hurtFX, _eyes.transform.position, Quaternion.identity);
			} 
			else
			{
				TransitionToState (DeadState);
				_isAlive = false;
			}
		}
	}

	public void Attack()
	{
		if (_characterStats)
		{
			if (period > _characterStats.AttackRate)
			{
				_animator.SetTrigger ("attack");

				if (_playerControls)
					_playerControls.StartCoroutine (_playerControls.Hurt (transform,_characterStats.AttackStrength));

				period = 0;
			}

			period += Time.deltaTime;
		}
	}

	public void TransitionToState(State pNextState)
	{
		if (pNextState != RemainState)
		{
			navMeshAgent.isStopped = true;
			_currentState = pNextState;
			_animator.SetTrigger (_currentState.AnimationTrigger);
			OnExitState ();
		}
	}

	public bool CheckIfCountDownElapsed(float duration)
	{
		StateTimeElapsed += Time.deltaTime;
		return (StateTimeElapsed >= duration);
	}

	private void OnExitState()
	{
		StateTimeElapsed = 0;
	}
}
