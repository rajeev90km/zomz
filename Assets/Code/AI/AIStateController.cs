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

	private int _nextWayPoint;
	public int NextWayPoint
	{
		get{ return _nextWayPoint; }
		set{ _nextWayPoint = value; }
	}

	private LineRenderer _lineRenderer;
	private List<Vector3> points = new List<Vector3> ();

	private int _groundLayerMask;

	void Start () 
	{
		_groundLayerMask |= (1 << LayerMask.NameToLayer ("Ground"));
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

	void Update () 
	{ 
		if (_isAlive)
		{
			if(_isAIOn)
				CurrentState.UpdateState (this);


			//ZOMZ
			if(Input.GetKeyDown(KeyCode.Z))
				ToggleAI();

			if (_beingControlled)
			{
				_normalModeModel.SetActive (false);
				_zomzModeModel.SetActive (true);

				if (Input.GetMouseButtonDown (0))
				{
					points.Clear ();
				}	

				if (Input.GetMouseButton (0))
				{
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
					RaycastHit hit;

					if (!points.Any ())
					{
						if (Physics.Raycast (ray, out hit))
						{
							if (hit.collider.gameObject == gameObject)
							{
								points.Add (hit.point);

								_lineRenderer.positionCount = points.Count;
								_lineRenderer.SetPositions (points.ToArray ());
							}
						}
					} else
					{
						if (Physics.Raycast (ray, out hit, Mathf.Infinity, _groundLayerMask))
						{
							if (DistanceToLastPoint (hit.point) > 0.25f)
							{
								points.Add (hit.point);

								_lineRenderer.positionCount = points.Count;
								_lineRenderer.SetPositions (points.ToArray ());
							}
						}
					}


				} else if (Input.GetMouseButtonUp (0))
				{

				}
			} 
			else
			{
				_normalModeModel.SetActive (true);
				_zomzModeModel.SetActive (false);

				points.Clear ();
				_lineRenderer.positionCount = points.Count;
				_lineRenderer.SetPositions (points.ToArray ());

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
