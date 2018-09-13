using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum JalaliStates
{
    NONE = -1,
    RUN = 0,
    SHOOT = 1,
    RELOAD = 2,
    ATTACK = 4,
    HURT = 5,
    DIE = 6,
    KICK = 7
}

public enum JalaliPhase
{
    PHASE_ONE = 0,
    PHASE_TWO = 1,
}

[System.Serializable]
public class GunTransforms
{
    public Vector3 Position;
    public Vector3 Rotation;
}

public class Jalali : Being
{
    private bool _isShooting = false;
    public bool IsShooting
    {
        get { return _isShooting; }
        set { _isShooting = value; }
    }

    private bool _isReloading = false;
    public bool IsReloading
    {
        get { return _isReloading; }
        set { _isReloading = value; }
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
    private JalaliPhase _currentPhase;
    public JalaliPhase CurrentPhase
    {
        get { return _currentPhase; }
    }

    [SerializeField]
    private JalaliStates _initState;
    public JalaliStates InitState
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

    protected JalaliStates _previousState = JalaliStates.NONE;

    [SerializeField]
    private GameObject _gun;

    [Header("Shoot Characteristics")]
    [SerializeField]
    private float _timeBetweenShots;

    [SerializeField]
    private float _shootTime;

    [SerializeField]
    private AnimationCurve _shootCurve;

    [SerializeField]
    private float _aimStartFactor = 2f;

    [SerializeField]
    private float _aimFollowSpeed = 1f;

    [SerializeField]
    private float _reloadTime = 4f;

    private Vector3 _aimFollowPosition = Vector3.zero;

    [SerializeField]
    private GameObject _gunShotBulletImpactFx;

    [Header("Gun Transforms")]
    [SerializeField]
    private GunTransforms _normalTransform;

    [SerializeField]
    private GunTransforms _shootTransform;

    [SerializeField]
    protected JalaliStates _currentState;

    [Header("Miscellaneous")]
    [SerializeField]
    protected float _sightHeightMultiplier = 1f;

    [SerializeField]
    protected float playerSightHeight = 0f;

    [SerializeField]
    protected GameObject _hurtFx;

    [SerializeField]
    protected Image _healthBar;

    public ZomzData ZomzMode;


    private Coroutine _playerHurtCoroutine;
    protected Coroutine _attackCoroutine;
    protected Coroutine _hurtCoroutine;
    protected Coroutine _shootCoroutine;
    protected Coroutine _reloadCoroutine;

    protected JalaliStates _animState;
    protected Collider ownCollider;

    protected int humanLayerMask;
    protected int playerLayerMask;
    protected int zombieLayerMask;
    protected int finalLayerMask;

    protected Being targetBeing;

    bool unobstructedViewToBeing = false;

    protected virtual void Awake()
    {
        //Cache Properties
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        ownCollider = GetComponent<Collider>();

        //Set initial State
        _initState = JalaliStates.RUN;
        _currentState = _initState;
        InitNewState("run");
        _previousState = _currentState;

        //Cache Player Controls
        _player = GameObject.FindWithTag("Player");
        _playerController = _player.GetComponent<CharacterControls>();

        //Setup Init Variables
        _currentHealth = _characterStats.Health;

        //LayerMasks
        playerLayerMask = (1 << LayerMask.NameToLayer("Player"));

        finalLayerMask = playerLayerMask;
    }

    public override IEnumerator Attack()
    {
        yield return null;
    }

    public IEnumerator Shoot()
    {
        if (_isAlive && !_isShooting)
        {
            _aimFollowPosition = _player.transform.position;

            _isShooting = true;

            float t = 0;

            while(t < _shootTime)
            {
                float distanceFromPlayer = _shootCurve.Evaluate((Mathf.Lerp(0, 1, t / _shootTime))) * _aimStartFactor;

                Vector3 directionToPlayer = transform.position - _aimFollowPosition;

                transform.LookAt(new Vector3(_aimFollowPosition.x, transform.position.y, _aimFollowPosition.z));

                if (_gunShotBulletImpactFx)
                {
                    GameObject bulletFX = Instantiate(_gunShotBulletImpactFx);
                    bulletFX.transform.position = _aimFollowPosition + (directionToPlayer.normalized * distanceFromPlayer);

                    if(Vector3.Distance(bulletFX.transform.position, _player.transform.position)<0.1f){

                        if(_playerHurtCoroutine!=null){
                            StopCoroutine(_playerHurtCoroutine);
                            _playerHurtCoroutine = null;
                        }
                        _playerHurtCoroutine = _playerController.StartCoroutine(_playerController.Hurt(1f));
                    }
                }

                yield return new WaitForSeconds(_timeBetweenShots);
                t += _timeBetweenShots;
            }

            _aimFollowPosition = Vector3.zero;

            _isShooting = false;

            _reloadCoroutine = StartCoroutine(Reload());
        }

        yield return null;
    }

    public IEnumerator Reload()
    {
        if(_isAlive && !_isShooting)
        {
            _currentState = JalaliStates.RELOAD;
            _previousState = JalaliStates.RELOAD;
                
            _isReloading = true;

            _animator.SetTrigger("reload");

            yield return new WaitForSeconds(_reloadTime);

            _isReloading = false;
        }

        yield return null;
    }

    public override IEnumerator Hurt(float pDamage = 0)
    {
        yield return null;
    }


    protected virtual void Update()
    {
        if (_isAlive && !_isAttacking && !_isShooting && !_isReloading && !_isHurting && !ZomzMode.CurrentValue)
        {
            ExecuteAI();
        }

        if(_aimFollowPosition != Vector3.zero)
        {
            Vector3 newPos = Vector3.Lerp(_aimFollowPosition, _player.transform.position, _aimFollowSpeed * Time.deltaTime);
            _aimFollowPosition = newPos;
        }
    }


    //*********************************************************************************************************************************************************
    #region AIStateBehaviors
    void ExecuteAI()
    {
        finalLayerMask = humanLayerMask | playerLayerMask;

        float distanceToBeing = Vector3.Distance(transform.position, _player.transform.position);
        Vector3 beingDirection = new Vector3(_player.transform.position.x, playerSightHeight, _player.transform.position.z) - transform.position;
        float beingAngle = Vector3.Angle(beingDirection, transform.forward);
        unobstructedViewToBeing = false;

        RaycastHit hit;

        Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);

        ownCollider.enabled = false;
        if (Physics.Raycast(transform.position + transform.up * _sightHeightMultiplier, beingDirection, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.collider.name);

            if (hit.collider.CompareTag("Player"))
            {
                unobstructedViewToBeing = true;
            }
        }
        ownCollider.enabled = true;

        Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);

        if(unobstructedViewToBeing)
        {
            _animator.ResetTrigger("run");
            _currentState = JalaliStates.SHOOT;
            InitNewState("shoot", false);
            _previousState = _currentState;
        }
        //else if(!unobstructedViewToBeing && hit.collider.GetComponent<Breakable>()!=null)
        //{
            
        //}

        switch (_currentState)
        {
            case JalaliStates.RUN:
                RunState();
                break;
            case JalaliStates.SHOOT:
                ShootState();
                break;
            case JalaliStates.ATTACK:
                AttackState();
                break;
            case JalaliStates.RELOAD:
                ReloadState();
                break;
            default:
                break;
        }
    }

    void DieState()
    {
        if (_isAlive)
        {
            _isAlive = false;
            _animator.SetTrigger("die");
            if (ownCollider)
                ownCollider.enabled = false;

            if (_shootCoroutine != null)
            {
                StopCoroutine(_shootCoroutine);
                _shootCoroutine = null;
            }
        }
    }

    void RunState(){
        if(_isAlive)
        {
            if (_navMeshAgent.isActiveAndEnabled)
            {
                _navMeshAgent.speed = _characterStats.RunSpeed;

                _gun.transform.localPosition = _normalTransform.Position;
                _gun.transform.localRotation = Quaternion.Euler(_normalTransform.Rotation);

                if(_player!=null)
                    _navMeshAgent.destination = _player.transform.position;
                _navMeshAgent.isStopped = false;
            }
        }
    }

    void ShootState(){

        if (_navMeshAgent.isActiveAndEnabled)
        {
            if (!_isShooting)
            {
                _navMeshAgent.destination = transform.position;
                _navMeshAgent.isStopped = true;

                _gun.transform.localPosition = _shootTransform.Position;
                _gun.transform.localRotation = Quaternion.Euler(_shootTransform.Rotation);

                _shootCoroutine = StartCoroutine(Shoot());
            }
        }

    }

    void AttackState(){
        
    }

    void ReloadState(){
        
    }

    protected void InitNewState(string pStateAnimTrigger, bool pIsLoopedAnim = false)
    {
        if (_previousState != _currentState || pIsLoopedAnim)
            _animator.SetTrigger(pStateAnimTrigger);
    }
    #endregion
}
