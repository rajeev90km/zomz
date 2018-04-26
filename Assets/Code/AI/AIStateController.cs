using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateController : MonoBehaviour {

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

	private Coroutine _attackPlayerCoroutine;

	private int _nextWayPoint;
	public int NextWayPoint
	{
		get{ return _nextWayPoint; }
		set{ _nextWayPoint = value; }
	}

	void Start () 
	{
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
			CurrentState.UpdateState (this);
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
					_attackPlayerCoroutine = _playerControls.StartCoroutine (_playerControls.Hurt (transform,_characterStats.AttackStrength));

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
