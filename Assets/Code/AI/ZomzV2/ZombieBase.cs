using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum ZombieStates
{
    NONE = -1,
    PATROL = 0,
    CHASE = 1,
    ATTACK = 2,
    SPECIAL_ATTACK = 3,
    DIE = 4
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieBase : MonoBehaviour
{
    private bool _isAttacking = false;
    public bool IsAttacking
    {
        get { return _isAttacking; }
        set { _isAttacking = value; }
    }

    private bool _isHurting = false;
    public bool IsHurting
    {
        get { return _isHurting; }
        set { _isHurting = value; }
    }

    private bool _isBeingControlled = false;
    public bool IsBeingControlled
    {
        get { return _isBeingControlled; }
        set { _isBeingControlled = value; }
    }

    private bool _isAlive = true;
    public bool IsAlive
    {
        get { return _isAlive; }
        set { _isAlive = value; }
    }

    [SerializeField]
    private ZombieStates _initState;
    public ZombieStates InitState{
        get { return _initState; }
    }

    [SerializeField]
    private CharacterStats _characterStats;
    public CharacterStats CharacterStats
    {
        get { return _characterStats; }
    }

    public float _currentHealth;
    public float CurrentHealth{
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    protected Animator _animator;
    protected NavMeshAgent _navMeshAgent;

    protected GameObject _player;
    protected CharacterControls _playerController;

    [HideInInspector]
    protected List<Transform> _wayPoints = new List<Transform>();

    protected int _nextWayPoint;

    public ZomzData ZomzMode;

    protected ZombieStates _previousState = ZombieStates.NONE;
    protected ZombieStates _currentState;

    [Header("Model Details")]
    [SerializeField]
    private GameObject _model;
    private Renderer _modelRenderer;

    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private Material _zomzModeMaterial;

    [Header("Miscellaneous")]
    [SerializeField]
    private GameObject _hurtFx;

    [SerializeField]
    private Image _zombieHealthBar;

    private Coroutine _attackCoroutine;
    private Coroutine _hurtCoroutine;

	void Awake () 
    {
        //Cache Properties
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _modelRenderer = _model.GetComponent<Renderer>();

        //Set initial State
        _initState = ZombieStates.PATROL;
        _currentState = _initState;
        InitNewState("walk");
        _previousState = _currentState;

        //Cache Player Controls
        _player = GameObject.FindWithTag("Player");
        _playerController = _player.GetComponent<CharacterControls>();

        //Setup Init Variables
        _currentHealth = _characterStats.Health;

        //Waypoints
        GameObject _wayPointsObj = GameObject.FindWithTag("Waypoints");
        if (_wayPointsObj != null)
        {
            for (int i = 0; i < _wayPointsObj.transform.childCount; i++)
            {
                _wayPoints.Add(_wayPointsObj.transform.GetChild(i));
            }
        }


	}

    //*********************************************************************************************************************************************************
    #region AIStateBehaviors
    protected void InitNewState(string pStateAnimTrigger, bool pIsLoopedAnim = false)
    {
        if(_previousState!=_currentState || pIsLoopedAnim)
            _animator.SetTrigger(pStateAnimTrigger); 
    }

    //Patrol Around Waypoint Object placed in the Scene - Randomized
    protected virtual void PatrolState()
    {
        if (_isAlive && !_isBeingControlled)
        {

            if(_navMeshAgent.isActiveAndEnabled)
            {
                _navMeshAgent.speed = _characterStats.WalkSpeed;
                _navMeshAgent.destination = _wayPoints[_nextWayPoint].position;
                _navMeshAgent.isStopped = false;

                if (_navMeshAgent.remainingDistance <= 1f)
                    GetNextWayPoint();
            }

        }
    }

    protected virtual void ChaseState()
    {
        if (_isAlive && !_isBeingControlled)
        {
            if (_navMeshAgent.isActiveAndEnabled)
            {
                _navMeshAgent.speed = _characterStats.RunSpeed;
                _navMeshAgent.destination = _player.transform.position;
                _navMeshAgent.isStopped = false;
            }
        }
    }

    protected virtual void AttackState()
    {
        if (_isAlive)
        {
            _navMeshAgent.destination = transform.position;
            _navMeshAgent.isStopped = true;
            _isAttacking = true;
            _attackCoroutine = StartCoroutine(Attack());
        }
    }

    protected virtual void UseSkillState()
    {
        if (_isAlive)
        {
            //None here for now. Override in Child Classes    
        }
    }

    protected virtual void DieState()
    {
        _isAlive = false;
        _animator.SetTrigger("die");
    }
	
    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected virtual void ExecuteAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        //Transition to CHASE mode if close enough to the player
        if (_playerController.IsAlive && (((distanceToPlayer < _characterStats.LookRange) && (distanceToPlayer > _characterStats.AttackRange)) || (!_isAttacking && _previousState == ZombieStates.ATTACK && distanceToPlayer > _characterStats.AttackRange)))
        {
            _currentState = ZombieStates.CHASE;
            InitNewState("run",false);
            _previousState = _currentState;
        }
        //Transition to ATTACK if in attack range
        else if (_playerController.IsAlive && (distanceToPlayer <= _characterStats.AttackRange))
        {
            transform.LookAt(_player.transform);
            _currentState = ZombieStates.ATTACK;
            InitNewState("attack",true);
            _previousState = _currentState;
        }
        //Transition to PATROL if it doesn't meet any of the criteria
        else if(!_isAttacking && distanceToPlayer > _characterStats.LookRange && _previousState == ZombieStates.CHASE)
        {
            _currentState = ZombieStates.PATROL;
            InitNewState("walk");
            _previousState = _currentState;    
        }
        else if(!_playerController.IsAlive)
        {
            _currentState = ZombieStates.PATROL;
            InitNewState("walk");
            _previousState = _currentState;    
        }
        else
        {
            _currentState = ZombieStates.PATROL;
            InitNewState("walk");
            _previousState = _currentState;    
        }


        switch(_currentState)
        {
            case ZombieStates.PATROL:
                PatrolState();
                break;
            case ZombieStates.CHASE:
                ChaseState();
                break;
            case ZombieStates.ATTACK:
                AttackState();
                break;
            default:
                break;
        }
    }
    #endregion


    //*********************************************************************************************************************************************************
    #region ZombieActions

    public virtual IEnumerator Attack()
    {
        yield return new WaitForSeconds(_characterStats.AttackRate / 2);

        if(Vector3.Distance(_player.transform.position,transform.position)<=_characterStats.AttackRange && !_isHurting)
        {
            StartCoroutine(_playerController.Hurt(_characterStats.AttackStrength));
        }

        yield return StartCoroutine(Hurt(_characterStats.AttackDamageToSelf));

        yield return new WaitForSeconds(_characterStats.AttackRate/2);
        _isAttacking = false;

        yield return null;
    }


    public virtual IEnumerator Hurt(float pDamage = 0.0f)
    {
        if (_isAlive)
        {
            if (pDamage > 0)
            {
                if (_attackCoroutine != null)
                {
                    StopCoroutine(_attackCoroutine);
                    _attackCoroutine = null;
                }

                _isHurting = true;
                _animator.SetTrigger("hurt");
                
                if (_currentHealth - pDamage > 0)
                    _currentHealth -= pDamage;
                else
                    _currentHealth = 0;

                if (_zombieHealthBar)
                    _zombieHealthBar.fillAmount = _currentHealth / 100;

                if (_hurtFx != null)
                    Instantiate(_hurtFx, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity);

                yield return new WaitForSeconds(_characterStats.HurtRate);

                _isHurting = false;
                _isAttacking = false;
                
                if (_currentHealth <= 0)
                {
                    DieState();
                }
            }
        }

        yield return null;
    }


    #endregion

    #region ZomzMode
    public void StartZomzMode()
    {
        _animator.SetTrigger("idle");
        _previousState = ZombieStates.NONE;
        _navMeshAgent.destination = transform.position;
        _navMeshAgent.isStopped = true;
    }

    public void EndZomzMode()
    {
        _modelRenderer.material = _defaultMaterial;
    }

    public void OnZomzModeAffected()
    {
        _modelRenderer.material = _zomzModeMaterial;
    }

    public void OnZomzModeRegister()
    {
        _isBeingControlled = true;
        _modelRenderer.material = _zomzModeMaterial;
    }

    public void OnZomzModeUnRegister()
    {
        _isBeingControlled = false;
        _modelRenderer.material = _zomzModeMaterial;
    }

    #endregion



    protected void GetNextWayPoint()
    {
        _nextWayPoint = Random.Range(0, _wayPoints.Count);
    }
	
	void Update () 
    {
        if(_isAlive && !_isBeingControlled && !_isAttacking && !_isHurting && !ZomzMode.CurrentValue)
            ExecuteAI();
	}
}
