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
    DIE = 4,
    HURT = 5,
    FLEE = 6,
}

public enum AttackTarget
{
    PLAYER = 0,
    ENEMY = 1
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieBase : Being
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

    private bool _isChaseOverridden = false;
    public bool IsChaseOverridden{
        get { return _isChaseOverridden; }
        set { _isChaseOverridden = value; }
    }

    private Vector3 overriddenChasePosition;
    public Vector3 OverridenChasePosition{
        get { return overriddenChasePosition; }
        set { overriddenChasePosition = value; }
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

    [SerializeField]
    protected ZombieStates _currentState;

    [Header("Model Details")]
    [SerializeField]
    private GameObject _model;
    protected Renderer _modelRenderer;

    [SerializeField]
    protected Material _defaultMaterial;

    [SerializeField]
    protected Material _zomzModeMaterial;

    [Header("Miscellaneous")]
    [SerializeField]
    protected GameFloatAttribute _zomzManaAttribute;

    [SerializeField]
    protected float _moveCostPerUnit = 1f;

    [SerializeField]
    protected float _attackCost = 10f;

    [SerializeField]
    protected GameObject _hurtFx;

    [SerializeField]
    protected Image _zombieHealthBar;

    protected Coroutine _attackCoroutine;
    protected Coroutine _hurtCoroutine;

    private float _currentSpeed;
    private float _speedSmoothTime = 0.1f;
    private float _speedSmoothVelocity;
    Vector3 forward, right;
    protected ZombieStates _animState;

    ZomzController _zomzControl;

	protected virtual void Awake () 
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

        _zomzControl = GameObject.FindWithTag("Player").GetComponent<ZomzController>();

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

                if (!_isChaseOverridden)
                    _navMeshAgent.destination = _player.transform.position;
                else
                    _navMeshAgent.destination = overriddenChasePosition;
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

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

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
        _zomzControl.UnregisterZomzMode();
    }
	
    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected virtual void ExecuteAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
        float distanceToChasePosition = Vector3.Distance(transform.position, overriddenChasePosition);

        //Reset Chase override if close to the patrol position
        if(_isChaseOverridden)
        {
            if (distanceToChasePosition < 2f || (distanceToPlayer <= _characterStats.AttackRange) || ((distanceToPlayer < _characterStats.LookRange) && (distanceToPlayer > _characterStats.AttackRange)) || (!_isAttacking && _previousState == ZombieStates.ATTACK && distanceToPlayer > _characterStats.AttackRange))
                _isChaseOverridden = false;
        }

        //Transition to CHASE mode if close enough to the player
        if (_playerController.IsAlive && ( _isChaseOverridden || ((distanceToPlayer < _characterStats.LookRange) && (distanceToPlayer > _characterStats.AttackRange)) || (!_isAttacking && _previousState == ZombieStates.ATTACK && distanceToPlayer > _characterStats.AttackRange)))
        {
            _currentState = ZombieStates.CHASE;
            InitNewState("run",false);  
            _previousState = _currentState;
        }
        //Transition to ATTACK if in attack range
        else if (_playerController.IsAlive && (distanceToPlayer <= _characterStats.AttackRange))
        {
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

    public override IEnumerator Attack()
    {
        if (!_isAttacking)
        {
            _isAttacking = true;

            if (_isBeingControlled)
                _animator.SetTrigger("attack");

            yield return new WaitForSeconds(_characterStats.AttackRate / 2);

            if (!_isBeingControlled)
            {
                transform.LookAt(_player.transform);

                if (Vector3.Distance(_player.transform.position, transform.position) <= _characterStats.AttackRange && !_isHurting)
                {
                    StartCoroutine(_playerController.Hurt(_characterStats.AttackStrength));
                }
            }
            else
            {
                if(ZomzMode.ManaConsumeType==ZomzManaConsumeType.ACTION_BASED)
                    _zomzManaAttribute.CurrentValue -= _attackCost;

                _animState = ZombieStates.ATTACK;

                GameObject closestEnemy = GetClosestObject();

                if (closestEnemy)
                {
                    transform.LookAt(closestEnemy.transform);    
                    ZombieBase zombieControls = closestEnemy.GetComponent<ZombieBase>();

                    if (zombieControls)
                        StartCoroutine(zombieControls.Hurt(_characterStats.AttackStrength));
                }

            }

            yield return new WaitForSeconds(_characterStats.AttackRate / 2);
            _isAttacking = false;

            //DAMAGE TO SELF
            if (_isAlive)
            {
                if (_currentHealth - _characterStats.AttackDamageToSelf > 0)
                    _currentHealth -= _characterStats.AttackDamageToSelf;
                else
                    _currentHealth = 0;

                if (_currentHealth <= 0)
                    DieState();
            }
        }
        yield return null;
    }


    public override IEnumerator Hurt(float pDamage = 0.0f)
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

                _previousState = ZombieStates.HURT;
                _currentState = ZombieStates.HURT;

                _isHurting = true;
                _animator.SetTrigger("hurt");
                _navMeshAgent.isStopped = true;
                
                if (_currentHealth - pDamage > 0)
                    _currentHealth -= pDamage;
                else
                    _currentHealth = 0;

                if (_zombieHealthBar)
                    _zombieHealthBar.fillAmount = _currentHealth / CharacterStats.Health;

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
    public virtual void StartZomzMode()
    {
        if (_isAlive)
        {
            _animator.SetTrigger("idle");
            _previousState = ZombieStates.NONE;
            _navMeshAgent.destination = transform.position;
            _navMeshAgent.isStopped = true;
        }
    }

    public virtual void EndZomzMode()
    {
        _modelRenderer.material = _defaultMaterial;
    }

    public virtual void OnZomzModeAffected()
    {
        _modelRenderer.material = _zomzModeMaterial;
    }

    public virtual void OnZomzModeRegister()
    {
        _isBeingControlled = true;
        _modelRenderer.material = _zomzModeMaterial;
        ResetDirectionVectors();
    }

    public virtual void OnZomzModeUnRegister()
    {
        _isBeingControlled = false;
        _modelRenderer.material = _defaultMaterial;
    }

    public void ResetDirectionVectors()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }

    public GameObject GetClosestObject()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _characterStats.AttackRange);
        Collider closestCollider = null;

        foreach (Collider hit in colliders)
        {
            ZombieBase zombieControls = hit.gameObject.GetComponent<ZombieBase>();

            if ((hit.GetComponent<Collider>() == transform.GetComponent<Collider>()) || !hit.transform.CompareTag("Enemy"))
            {
                continue;
            }

            if (zombieControls != null && !zombieControls.IsAlive)
            {
                continue;
            }

            if (!closestCollider)
            {
                closestCollider = hit;
            }
            //compares distances
            if (Vector3.Distance(transform.position, hit.transform.position) <= Vector3.Distance(transform.position, closestCollider.transform.position))
            {
                closestCollider = hit;
            }
        }

        if (!closestCollider)
            return null;

        return closestCollider.gameObject;
    }

    #endregion

    protected void GetNextWayPoint()
    {
        _nextWayPoint = Random.Range(0, _wayPoints.Count);
    }
	
    protected virtual void Update () 
    {
        if(_isAlive && !_isBeingControlled && !_isAttacking && !_isHurting && !ZomzMode.CurrentValue)
            ExecuteAI();

        //Zomz Mode Registered - MOVEMENt
        if(_isBeingControlled && !_isAttacking && !_isHurting)
        {
            bool running = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = ((running) ? _characterStats.RunSpeed : _characterStats.WalkSpeed);
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);

            Vector3 rightMovement = right * _currentSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
            Vector3 upMovement = forward * _currentSpeed * Time.deltaTime * Input.GetAxis("Vertical");

            Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

            if (heading != Vector3.zero)
            {
                transform.forward = heading;
                transform.position += rightMovement + upMovement;
            }

            float animationSpeedPercent = ((running) ? 1 : 0.5f) * heading.magnitude;

            if(ZomzMode.ManaConsumeType == ZomzManaConsumeType.ACTION_BASED)
                _zomzManaAttribute.CurrentValue -= animationSpeedPercent * _moveCostPerUnit;

            if (animationSpeedPercent == 0 && _animState != ZombieStates.NONE)
            {
                _animator.SetTrigger("idle");
                _animState = ZombieStates.NONE;
            }
            else if (animationSpeedPercent == 0.5f && _animState != ZombieStates.PATROL)
            {
                _animator.SetTrigger("walk");
                _animState = ZombieStates.PATROL;
            }
            if (animationSpeedPercent == 1f && _animState != ZombieStates.CHASE)
            {
                _animator.SetTrigger("run");
                _animState = ZombieStates.CHASE;
            }
        }

        //Zomz Registered - ATTACK
        if(_isBeingControlled)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Attack());
            }
        }

        if (!_playerController.IsAlive)
            _isBeingControlled = false;
	}
}
