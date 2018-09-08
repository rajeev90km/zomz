using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum HumanStates
{
    NONE = -1,
    WALK = 0,
    CHASE = 1,
    ATTACK = 2,
    DIE = 4,
    HURT = 5,
    FLEE = 6,
}

//TODO: When screamer screams nearby and you can't see, turn around
//TODO: Fleeing Behaviours
//TODO: Frozen in Fear ( but when attacked, attack back )
public class HumanBase : Being
{
    [SerializeField]
    private bool _canAttackHumans;
    public bool CanAttackHumans
    {
        get { return _canAttackHumans; }
    }

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

    [SerializeField]
    private HumanStates _initState;
    public HumanStates InitState
    {
        get { return _initState; }
    }

    [SerializeField]
    private CharacterStats _characterStats;
    public CharacterStats CharacterStats
    {
        get { return _characterStats; }
    }

    public float _currentHealth;
    public float CurrentHealth
    {
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

    protected HumanStates _previousState = HumanStates.NONE;

    [SerializeField]
    protected HumanStates _currentState;

    [Header("Miscellaneous")]
    [SerializeField]
    protected float _sightHeightMultiplier = 1f;

    [SerializeField]
    protected float playerSightHeight = 0f;

    [SerializeField]
    protected GameObject _hurtFx;

    [SerializeField]
    protected Image _humanHealthBar;

    public ZomzData ZomzMode;

    protected Coroutine _attackCoroutine;
    protected Coroutine _hurtCoroutine;

    protected HumanStates _animState;
    protected Collider ownCollider;

    protected int humanLayerMask;
    protected int playerLayerMask;
    protected int zombieLayerMask;
    protected int finalLayerMask;

    protected Being targetBeing;

    protected virtual void Awake()
    {
        //Cache Properties
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        ownCollider = GetComponent<Collider>();

        //Set initial State
        _initState = HumanStates.WALK;
        _currentState = _initState;
        InitNewState("walk");
        _previousState = _currentState;

        //Cache Player Controls
        _player = GameObject.FindWithTag("Player");
        _playerController = _player.GetComponent<CharacterControls>();

        //Setup Init Variables
        _currentHealth = _characterStats.Health;

        //LayerMasks
        humanLayerMask = (1 << LayerMask.NameToLayer("Human"));
        playerLayerMask = (1 << LayerMask.NameToLayer("Player"));
        zombieLayerMask = (1 << LayerMask.NameToLayer("Enemy"));

        finalLayerMask = zombieLayerMask;

        if (_canAttackHumans)
            finalLayerMask = finalLayerMask | humanLayerMask | playerLayerMask;

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
    #region Actions
    public override IEnumerator Attack()
    {
        if (!_isAttacking)
        {
            _isAttacking = true;

            Being closestEnemy = GetClosestBeingToAttack(finalLayerMask, _characterStats.AttackRange);

            if (closestEnemy)
                transform.LookAt(closestEnemy.transform);

            yield return new WaitForSeconds(_characterStats.AttackRate / 2);

            if (closestEnemy)
            {
                if (Vector3.Distance(closestEnemy.transform.position, transform.position) <= _characterStats.AttackRange && !_isHurting)
                {
                    closestEnemy.StartCoroutine(closestEnemy.Hurt(_characterStats.AttackStrength));
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

                if (_currentHealth <= 0.1f)
                    DieState();
            }
        }
        yield return null;
    }

    public override IEnumerator Hurt(float pDamage = 0)
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

                _previousState = HumanStates.HURT;
                _currentState = HumanStates.HURT;

                _isHurting = true;
                _animator.SetTrigger("hurt");
                _navMeshAgent.isStopped = true;

                if (_currentHealth - pDamage > 0)
                    _currentHealth -= pDamage;
                else
                    _currentHealth = 0;

                if (_humanHealthBar)
                    _humanHealthBar.fillAmount = _currentHealth / CharacterStats.Health;

                if (_hurtFx != null)
                    Instantiate(_hurtFx, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity);

                yield return new WaitForSeconds(_characterStats.HurtRate);

                _isHurting = false;
                _isAttacking = false;

                if (_currentHealth <= 0.1f)
                {
                    DieState();
                }
            }
        }

        yield return null;
    }
    #endregion


    protected virtual void Update()
    {
        if (_isAlive && !_isAttacking && !_isHurting && !ZomzMode.CurrentValue)
        {
            ExecuteAI();
        }
    }

    //*********************************************************************************************************************************************************
    #region AIStateBehaviors
    protected void InitNewState(string pStateAnimTrigger, bool pIsLoopedAnim = false)
    {
        if (_previousState != _currentState || pIsLoopedAnim)
            _animator.SetTrigger(pStateAnimTrigger);
    }

    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected virtual void ExecuteAI()
    {
        Being visibleBeing = GetBeingInLookRange(finalLayerMask, _characterStats.LookRange);
        float distanceToBeing = Mathf.Infinity;
        Vector3 beingDirection = Vector3.zero;
        float beingAngle = Mathf.Infinity;
        bool unobstructedViewToBeing = false;

        if (visibleBeing != null)
        {
            distanceToBeing = Vector3.Distance(transform.position, visibleBeing.transform.position);
            beingDirection = visibleBeing.transform.position - transform.position;
            beingAngle = Vector3.Angle(beingDirection, transform.forward);

            if (visibleBeing.CompareTag("Player"))
            {
                RaycastHit hit;

                Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);

                ownCollider.enabled = false;
                if (Physics.Raycast(transform.position + transform.up * _sightHeightMultiplier, beingDirection, out hit, Mathf.Infinity))
                {
                    if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Human"))
                    {
                        unobstructedViewToBeing = true;
                    }
                }
                ownCollider.enabled = true;
            }
            else
                unobstructedViewToBeing = true;
        }

        if (visibleBeing == null)
        {
            _currentState = HumanStates.WALK;
            InitNewState("walk");
            _previousState = _currentState;
        }
        //Transition to CHASE mode if close enough to the player
        else if (visibleBeing != null && visibleBeing.IsAlive && unobstructedViewToBeing && !_isAttacking && distanceToBeing > _characterStats.AttackRange && beingAngle < _characterStats.FieldOfView * 0.5f)
        {
            targetBeing = visibleBeing;
            _animator.ResetTrigger("walk");
            _currentState = HumanStates.CHASE;
            InitNewState("run", false);
            _previousState = _currentState;
        }
        //Transition to ATTACK if in attack range
        else if ((distanceToBeing <= _characterStats.AttackRange))
        {
            _animator.ResetTrigger("walk");
            _currentState = HumanStates.ATTACK;
            InitNewState("attack", true);
            _previousState = _currentState;
        }
        else
        {
            _currentState = HumanStates.WALK;
            InitNewState("walk");
            _previousState = _currentState;
        }


        switch (_currentState)
        {
            case HumanStates.WALK:
                WalkState();
                break;
            case HumanStates.CHASE:
                ChaseState();
                break;
            case HumanStates.ATTACK:
                AttackState();
                break;
            case HumanStates.FLEE:
                FleeState();
                break;
            default:
                break;
        }
    }

    protected virtual void WalkState()
    {
        if (_isAlive)
        {

            if (_navMeshAgent.isActiveAndEnabled)
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
        if (_isAlive)
        {
            if (_navMeshAgent.isActiveAndEnabled)
            {
                _navMeshAgent.speed = _characterStats.RunSpeed;

                if (targetBeing != null)
                    _navMeshAgent.destination = targetBeing.transform.position;
                _navMeshAgent.isStopped = false;
            }
        }
    }

    protected virtual void AttackState()
    {
        if (_isAlive && !_isAttacking)
        {
            _navMeshAgent.destination = transform.position;
            _navMeshAgent.isStopped = true;

            _attackCoroutine = StartCoroutine(Attack());
        }
    }

    protected virtual void DieState()
    {
        if (_isAlive)
        {
            _isAlive = false;
            _animator.SetTrigger("die");
            ownCollider.enabled = false;
        }
    }

    protected virtual void FleeState()
    {

    }
    #endregion


    //*********************************************************************************************************************************************************
    #region Utils
    protected void GetNextWayPoint()
    {
        _nextWayPoint = Random.Range(0, _wayPoints.Count);
    }

    protected void GetExclusiveNextWayPoint()
    {
        _nextWayPoint = (_wayPoints.Count - _nextWayPoint) % _wayPoints.Count;
    }
    #endregion



    //**********************************************************************************************************************************************************
    #region ZomzMode
    public virtual void StartZomzMode()
    {
        if (_isAlive)
        {
            _animator.SetTrigger("idle");
            _previousState = HumanStates.NONE;
            _navMeshAgent.destination = transform.position;
            _navMeshAgent.isStopped = true;
        }
    }

    public virtual void EndZomzMode()
    {
        
    }
    #endregion
}
