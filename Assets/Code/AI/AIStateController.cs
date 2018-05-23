using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
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
    private GameData _gameData;

	[SerializeField]
	private CharacterStats _characterStats;
	public CharacterStats CharacterStats
	{
		get { return _characterStats; }
	}

	[SerializeField]
	private ZomzListAttribute _zomzActionsList;

    [SerializeField]
    private SelectedZombie _currentSelectedZombie;

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
	private Image _zombieHealthBar;

	[Header("Models")]
	[SerializeField]
	private GameObject _normalModeModel;
	[SerializeField]
	private GameObject _zomzModeModel;
	[SerializeField]
	private Material _normalModeMaterial;
	[SerializeField]
	private Material _zomzModeMaterial;
	[SerializeField]
	private GameObject _selectionQuad;

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

	[Header("Mana Costs")]
	[SerializeField]
	private GameFloatAttribute _zomzManaAttribute;

	[SerializeField]
	private float _manaForUnitMovement = 2f;

	[SerializeField]
	private float _manaForAttack = 10f;

	private LineRenderer _lineRenderer;

	private Queue<ZomzActionPoint> _zomzActionPoints = new Queue<ZomzActionPoint>();


	private List<Vector3> points = new List<Vector3> ();

	private int _groundLayerMask;
	private int _enemyPlayerMask;
	private ZomzActionSystem _zactionSystem;
	private Renderer _renderer;

	private bool _isExecutingActions = false;


	private Coroutine _zomzAttackCoroutine;
	private Coroutine _hurtPlayerCoroutine;

	void Start () 
	{
		_renderer = _normalModeModel.GetComponent<Renderer> ();
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

        _currentSelectedZombie.CurrentSelectedZombie = this;

		if (_selectionQuad)
			_selectionQuad.SetActive (true);
	}

	public void ClearCurrentControl()
	{

        //_currentSelectedZombie.ResetSelection();

		_selectedForControl = false;
		if (_selectionQuad)
			_selectionQuad.SetActive (false);
	}

	public void TakeControl()
	{
		_renderer.sharedMaterial = _zomzModeMaterial;

		if(_zactionSystem)
			_zactionSystem.enabled = true;
		

		_beingControlled = true;
        _zomzActionsList.AllActionPoints.Clear ();
		_zomzActionPoints.Enqueue(new ZomzActionPoint(_zomzModeModel.transform.position,ZomzAction.MOVE,null));
        ToggleZomzAttackMode(false);
        //_zomzActionsList.AllActionPoints.Add(new ZomzActionPoint(this,_zomzModeModel.transform.position, ZomzAction.MOVE, null));

		points.Clear ();
        points.Add (_zomzModeModel.transform.position);
	}

    public void BeforeExecuting()
    {
        ClearCurrentControl();
        _zomzModeModel.SetActive(false);
        _zactionSystem.IsSelected = false;
        _isExecutingActions = true;    
        _currentSelectedZombie.ResetSelection();

        //points.Clear();
        //_lineRenderer.positionCount = points.Count;
        //_lineRenderer.SetPositions(points.ToArray());

        EnableDisableColliders(false);
    }

    void EnableDisableColliders(bool pEnable)
    {
        Collider coll = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();

        if (coll != null)
            coll.enabled = pEnable;

        if(rb!=null)
        {
            if(!pEnable)
                rb.constraints = RigidbodyConstraints.FreezePosition;
            else
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
        }
        
    }

	public void RelinquishControl()
	{
		_isExecutingActions = false;
		_renderer.sharedMaterial = _normalModeMaterial;
		Time.timeScale = 1;
		_beingControlled = false;
		_zomzModeModel.transform.localPosition = Vector3.zero;
		_zomzModeModel.transform.localRotation = Quaternion.identity;
		_zomzActionPoints.Clear ();
		points.Clear ();
		navMeshAgent.speed = _characterStats.WalkSpeed;
        EnableDisableColliders(true);
        ToggleAI(true);
	}


	public void DealZomzDamage(Transform pTarget)
	{
		if (pTarget.CompareTag ("Enemy"))
		{
			AIStateController otherZombie = pTarget.GetComponent<AIStateController> ();
			if (otherZombie != null)
			{
				otherZombie.TakeDamage (_characterStats.AttackStrength);
                TakeDamage(_characterStats.AttackDamageToSelf);
			}
		}

		if (pTarget.CompareTag ("Player"))
		{
            if (_playerControls)
            {
                _playerControls.StartCoroutine(_playerControls.Hurt(transform, _characterStats.AttackStrength));
                TakeDamage(_characterStats.AttackDamageToSelf);
            }
		}


	}

	IEnumerator WaitToEndZomzAttack(Transform pTarget)
	{
		yield return new WaitForSeconds (_characterStats.AttackRate);

		_zomzAttackCoroutine = null;
		_animator.SetTrigger ("walk");
	}

    void Update()
    {
        if(!_gameData.IsPaused)
        {
            if (_isAlive)
            {
                if (_isAIOn)
                    CurrentState.UpdateState(this);

                if (_zomzManaAttribute.CurrentValue > 0)
                {
                    //Under Zomz mode and selected by clicking
                    if (_beingControlled && _selectedForControl)
                    {
                        _zomzModeModel.SetActive(true);
                        _zactionSystem.IsSelected = true;

                        if (DistanceToLastPoint(_zomzModeModel.transform.position) > 0.5f)
                        {
                            if (_zomzManaAttribute)
                                _zomzManaAttribute.CurrentValue -= _manaForUnitMovement;

                            //_zomzActionPoints.Enqueue (new ZomzActionPoint (_zomzModeModel.transform.position, ZomzAction.MOVE, null));
                            _zomzActionsList.AllActionPoints.Add(new ZomzActionPoint(this, _zomzModeModel.transform.position, ZomzAction.MOVE, null));
                            ToggleZomzAttackMode(false);
                            points.Add(_zomzActionsList.AllActionPoints.Last().Position);
                            _lineRenderer.positionCount = points.Count;
                            _lineRenderer.SetPositions(points.ToArray());
                        }

                        //Show attack sphere
                        if (Input.GetKeyDown(KeyCode.Alpha1))
                        {
                            ToggleZomzAttackMode(true);
                            //_zomzAttack = true;
                        }

                        if (_zomzAttack)
                        {
                            if (Input.GetMouseButtonDown(1))
                            {
                                RaycastHit hit;
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity, _enemyPlayerMask))
                                {
                                    if (hit.transform != null)
                                    {
                                        if (hit.collider.gameObject != this.gameObject)
                                        {

                                            if (Vector3.Distance(_zomzModeModel.transform.position, hit.transform.position) <= _characterStats.AttackRange)
                                            {
                                                if (_zomzManaAttribute)
                                                    _zomzManaAttribute.CurrentValue -= _manaForAttack;

                                                //_zomzActionPoints.Enqueue (new ZomzActionPoint (_zomzModeModel.transform.position, ZomzAction.ATTACK, hit.transform));
                                                _zomzActionsList.AllActionPoints.Add(new ZomzActionPoint(this, _zomzModeModel.transform.position, ZomzAction.ATTACK, hit.transform));
                                                _zactionSystem.Animator.SetTrigger("attack");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _zactionSystem.enabled = false;
                }

                //Released from zomz mode
                if (!_beingControlled)
                {
                    _zomzModeModel.SetActive(false);
                    _zactionSystem.IsSelected = false;
                    _zomzActionPoints.Clear();
                    points.Clear();
                    _lineRenderer.positionCount = points.Count;
                    _lineRenderer.SetPositions(points.ToArray());
                    _selectedForControl = false;
                    ToggleZomzAttackMode(false);
                }

                //Different Zombie Selected
                if (!_selectedForControl)
                {
                    _zactionSystem.IsSelected = false;
                }

            }
        }
	}

    public void ToggleZomzAttackMode(bool pEnable)
    {
        _zomzAttack = pEnable;
    }

	private float DistanceToLastPoint(Vector3 pPoint)
	{
        if (!_zomzActionsList.AllActionPoints.Any())
			return float.MaxValue;
        return Vector3.Distance (_zomzActionsList.AllActionPoints.Last().Position, pPoint);
	}

	void ResetAI()
	{
		navMeshAgent.isStopped = true;
        _currentState = null;
		_animator.SetTrigger ("idle");
	}

	public void ToggleAI(bool pOnOff)
	{
        if (_isAlive && !_isExecutingActions)
		{
			_isAIOn = pOnOff;	

			if (!_isAIOn)
			{
				ResetAI ();
			} 
            else
			{
				TransitionToState (InitState);
			}
		}

	}

	void OnDrawGizmos()
	{
		if (_currentState != null)
		{
			Gizmos.color = _currentState.SceneGizmoColor;
			Gizmos.DrawWireSphere (transform.position, 2);
		}

		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere (transform.position, _characterStats.ZomzRange);
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
			{
				_currentHealth -= pDamage;
			}
			else
				_currentHealth = 0;

			if (_currentHealth > 0)
			{
				yield return new WaitForSeconds(0.7f);

				if (_hurtFX != null)
					Instantiate (_hurtFX, _eyes.transform.position, Quaternion.identity);

				//Update UI Element
				if (_zombieHealthBar)
					_zombieHealthBar.fillAmount = _currentHealth / 100;
			} 
			else
			{
				yield return new WaitForSeconds(0.7f);

				//Update UI Element
				if (_zombieHealthBar)
					_zombieHealthBar.fillAmount = 0;

				TransitionToState (DeadState);
				_isAlive = false;
			}
		}
	}

	public void Attack()
	{
		if (!IsAlive)
		{
			StopCoroutine (_hurtPlayerCoroutine);
			_hurtPlayerCoroutine = null;
			return;
		}

		if (_characterStats)
		{
			if (period > _characterStats.AttackRate)
			{
				_animator.SetTrigger ("attack");

				if (_playerControls)
				{
					_playerControls.StartCoroutine (_playerControls.Hurt (transform, _characterStats.AttackStrength));

                    if(_characterStats.AttackDamageToSelf>0)
                        TakeDamage(_characterStats.AttackDamageToSelf);
				}

				period = 0;
			}

			period += Time.deltaTime;
		}
	}

	public void TransitionToState(State pNextState)
	{
		if (pNextState != RemainState)
		{
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
