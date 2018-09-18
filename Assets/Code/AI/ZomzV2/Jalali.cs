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
    PHASE_THREE = 2,
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

    [SerializeField]
    private float _damagePerShot = 1f;

    [SerializeField]
    private float _damagePerZombieShot = 2f;

    private Vector3 _aimFollowPosition = Vector3.zero;

    [SerializeField]
    private GameObject _gunShotBulletImpactFx;

    [SerializeField]
    private GameObject _muzzleFlash;

    [Header("Gun Transforms")]
    [SerializeField]
    private GunTransforms _normalTransform;

    [SerializeField]
    private GunTransforms _shootTransform;

    [SerializeField]
    private GunTransforms _runTransform;

    [SerializeField]
    public JalaliStates _currentState;

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

    private LineRenderer _shotLine;

    private bool _inZomzMode = false;


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

    private int[] _phaseThreeShootDirections = { 1, -1 };

    bool allVantagePointsRayCast = false;

    bool unobstructedViewToBeing = false;

    int vantagePointIndex = -1;

    List<Transform> jalaliVantagePoints = new List<Transform>();


    private Breakable currentTargetBreakable = null;
    private Being currentVisibleBeing = null;

    protected virtual void Awake()
    {
        //Cache Properties
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        ownCollider = GetComponent<Collider>();
        _shotLine = GetComponent<LineRenderer>();

        //Set initial State
        _initState = JalaliStates.NONE;
        _currentState = _initState;
        InitNewState("idle");
        _previousState = _currentState;

        //Cache Player Controls
        _player = GameObject.FindWithTag("Player");
        _playerController = _player.GetComponent<CharacterControls>();

        //Setup Init Variables
        _currentHealth = _characterStats.Health;

        //LayerMasks
        playerLayerMask = (1 << LayerMask.NameToLayer("Player"));

        finalLayerMask = playerLayerMask;

        GameObject JalaliWayPoints = GameObject.FindWithTag("JalaliVantagePoints");

        if(JalaliWayPoints!=null)
        {
            for (int i = 0; i < JalaliWayPoints.transform.childCount;i++)
            {
                jalaliVantagePoints.Add(JalaliWayPoints.transform.GetChild(i));   
            }
        }
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

            int shootDirection = _phaseThreeShootDirections[Random.Range(0, _phaseThreeShootDirections.Length)];

            while (t < _shootTime && !ZomzMode.CurrentValue)
            {
                float distanceFromPlayer = _shootCurve.Evaluate((Mathf.Lerp(0, 1, t / _shootTime))) * _aimStartFactor;

                Vector3 directionToPlayer = Vector3.zero;

                if(_currentPhase==JalaliPhase.PHASE_ONE || _currentPhase == JalaliPhase.PHASE_TWO)
                    directionToPlayer = transform.position - _aimFollowPosition;
                else
                    directionToPlayer = shootDirection * transform.right;
                
                Vector3 beingDirection = new Vector3(_player.transform.position.x, playerSightHeight, _player.transform.position.z) - transform.position;

                transform.LookAt(new Vector3(_aimFollowPosition.x, transform.position.y, _aimFollowPosition.z));

                //Check for each shot if can see player
                RaycastHit hit;
                bool canSeePlayer = false;

                //Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);


                ownCollider.enabled = false;
                if (Physics.Raycast(transform.position + transform.up * _sightHeightMultiplier, beingDirection, out hit, Mathf.Infinity))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        canSeePlayer = true;
                    }
                }
                ownCollider.enabled = true;
                //Check for each shot if can see player

                //If there is a breakable container that frame
                if (currentTargetBreakable!=null && !canSeePlayer)
                {
                    if (_gunShotBulletImpactFx)
                    {
                        _muzzleFlash.SetActive(true);

                        Vector3 directionToBreakable = Vector3.zero;

                        if (_currentPhase == JalaliPhase.PHASE_ONE || _currentPhase == JalaliPhase.PHASE_TWO)
                            directionToBreakable = transform.position - currentTargetBreakable.transform.position;
                        else
                            directionToBreakable = shootDirection * transform.right;

                        GameObject bulletFX = Instantiate(_gunShotBulletImpactFx);
                        bulletFX.transform.position = currentTargetBreakable.transform.position + (directionToBreakable.normalized * distanceFromPlayer);

                        _shotLine.SetPosition(0, _muzzleFlash.transform.position);
                        _shotLine.SetPosition(1, bulletFX.transform.position);

                        //Debug.Log(currentTargetBreakable.name);
                        //Debug.Log(Vector3.Distance(bulletFX.transform.position, currentTargetBreakable.transform.position));

                        if (Vector3.Distance(bulletFX.transform.position, currentTargetBreakable.transform.position) < 0.1f)
                        {
                            if (currentTargetBreakable.Durability > 0)
                                currentTargetBreakable.Damage(_damagePerShot);
                        }

                        if (currentTargetBreakable.Durability <= 0.1f)
                            currentTargetBreakable = null;
                    }

                    _shotLine.enabled = true;
                    yield return new WaitForSeconds(_timeBetweenShots / 2);
                    _shotLine.enabled = false;
                    yield return new WaitForSeconds(_timeBetweenShots / 2);
                }
                else if (currentVisibleBeing != null && !canSeePlayer)
                {
                    if (_gunShotBulletImpactFx)
                    {
                        _muzzleFlash.SetActive(true);

                        Vector3 directionToBeing = Vector3.zero;

                        if (_currentPhase == JalaliPhase.PHASE_ONE || _currentPhase == JalaliPhase.PHASE_TWO)
                            directionToBeing = transform.position - currentVisibleBeing.transform.position;
                        else
                            directionToBeing = shootDirection * transform.right;

                        GameObject bulletFX = Instantiate(_gunShotBulletImpactFx);
                        bulletFX.transform.position = currentVisibleBeing.transform.position + (directionToBeing.normalized * distanceFromPlayer);

                        _shotLine.SetPosition(0, _muzzleFlash.transform.position);
                        _shotLine.SetPosition(1, bulletFX.transform.position);

                        //Debug.Log(currentTargetBreakable.name);
                        //Debug.Log(Vector3.Distance(bulletFX.transform.position, currentTargetBreakable.transform.position));

                        if (Vector3.Distance(bulletFX.transform.position, currentVisibleBeing.transform.position) < 0.1f)
                        {
                            currentVisibleBeing.StartCoroutine(currentVisibleBeing.Hurt(_damagePerZombieShot));
                        }

                        if (!currentVisibleBeing.IsAlive)
                            currentVisibleBeing = null;
                    }

                    _shotLine.enabled = true;
                    yield return new WaitForSeconds(_timeBetweenShots / 2);
                    _shotLine.enabled = false;
                    yield return new WaitForSeconds(_timeBetweenShots / 2);
                }
                else if (canSeePlayer) 
                { 
                    if (_gunShotBulletImpactFx)
                    {
                        _muzzleFlash.SetActive(true);

                        GameObject bulletFX = Instantiate(_gunShotBulletImpactFx);
                        bulletFX.transform.position = _aimFollowPosition + (directionToPlayer.normalized * distanceFromPlayer);

                        _shotLine.SetPosition(0, _muzzleFlash.transform.position);
                        _shotLine.SetPosition(1, bulletFX.transform.position);

                        if (Vector3.Distance(bulletFX.transform.position, _player.transform.position) < 0.1f)
                        {
                            //if(!_playerController.IsHurting)
                            _playerController.StartCoroutine(_playerController.Hurt(_damagePerShot));
                        }
                    }

                    _shotLine.enabled = true;
                    yield return new WaitForSeconds(_timeBetweenShots/2);
                    _shotLine.enabled = false;
                    yield return new WaitForSeconds(_timeBetweenShots / 2);
                }
                else
                {
                    _muzzleFlash.SetActive(false);
                    _shotLine.enabled = false;
                }
                t += _timeBetweenShots;
            }

            if(ZomzMode.CurrentValue)
            {
                _muzzleFlash.SetActive(false);
                _shotLine.enabled = false;
            }

            _aimFollowPosition = Vector3.zero;

            _isShooting = false;

            if(!ZomzMode.CurrentValue)
                _reloadCoroutine = StartCoroutine(Reload());
        }

        yield return null;
    }

    public IEnumerator Reload()
    {
        if(_isAlive && !_isShooting)
        {
            _muzzleFlash.SetActive(false);

            _currentState = JalaliStates.RELOAD;
            _previousState = JalaliStates.RELOAD;
                
            _isReloading = true;

            _animator.SetTrigger("reload");

            yield return new WaitForSeconds(_reloadTime);

            _isReloading = false;

            allVantagePointsRayCast = false;
            vantagePointIndex = -1;
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

                _previousState = JalaliStates.HURT;
                _currentState = JalaliStates.HURT;

                _isHurting = true;
                _animator.SetTrigger("hurt");
                _navMeshAgent.isStopped = true;

                if (_currentHealth - pDamage > 0)
                    _currentHealth -= pDamage;
                else
                    _currentHealth = 0;

                if (_healthBar)
                    _healthBar.fillAmount = _currentHealth / CharacterStats.Health;

                if (_hurtFx != null)
                    Instantiate(_hurtFx, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity);

                yield return new WaitForSeconds(_characterStats.HurtRate);

                _isHurting = false;
                _isAttacking = false;

                if (_currentHealth <= 0.1)
                {
                    DieState();
                }
            }
        }

        yield return null;
    }


    protected virtual void Update()
    {
        if (_isAlive && !_isAttacking && !_isShooting && !_isReloading && !_isHurting && !_inZomzMode)
        {
            ExecuteAI();
        }

        if(_aimFollowPosition != Vector3.zero)
        {
            Vector3 newPos = Vector3.Lerp(_aimFollowPosition, _player.transform.position, _aimFollowSpeed * Time.deltaTime);
            _aimFollowPosition = newPos;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CharacterStats.ShootRange);
    }

    public void InZomzMode()
    {
        _inZomzMode = true;
        _muzzleFlash.SetActive(false);
        _currentState = JalaliStates.NONE;
        InitNewState("idle", false);
        _previousState = _currentState;
        allVantagePointsRayCast = false;
        vantagePointIndex = -1;
        _navMeshAgent.isStopped = true;
    }

    public void OutOfZomzMode()
    {
        _inZomzMode = false;
    }

    //*********************************************************************************************************************************************************
    #region AIStateBehaviors
    void ExecuteAI()
    {
        finalLayerMask = humanLayerMask | playerLayerMask;

        float distanceToBeing = Vector3.Distance(transform.position, _player.transform.position);
        Vector3 beingDirection = new Vector3(_player.transform.position.x, playerSightHeight, _player.transform.position.z) - transform.position;
        float beingAngle = Vector3.Angle(beingDirection, transform.forward);

        RaycastHit hit;

        Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);

        ownCollider.enabled = false;
        if (Physics.Raycast(transform.position + transform.up * _sightHeightMultiplier, beingDirection, out hit, Mathf.Infinity))
        {
            //Debug.Log(hit.collider.name);

            if (hit.collider.CompareTag("Player"))
            {
                unobstructedViewToBeing = true;
            }
            else
                unobstructedViewToBeing = false;
        }
        ownCollider.enabled = true;

        //Debug.Log(unobstructedViewToBeing);

        Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);

        if(unobstructedViewToBeing && distanceToBeing <= _characterStats.ShootRange && _playerController.IsAlive)
        {
            currentTargetBreakable = null;
            currentVisibleBeing = null;
            allVantagePointsRayCast = false;
            vantagePointIndex = -1;
            _animator.ResetTrigger("run");
            _currentState = JalaliStates.SHOOT;
            InitNewState("shoot", false);
            _previousState = _currentState;
        }
        else if(hit.collider.GetComponent<Breakable>() && distanceToBeing <= _characterStats.ShootRange && _playerController.IsAlive)
        {
            currentVisibleBeing = null;
            allVantagePointsRayCast = false;
            vantagePointIndex = -1;
            _animator.ResetTrigger("run");
            _currentState = JalaliStates.SHOOT;
            InitNewState("shoot", false);
            _previousState = _currentState;

            currentTargetBreakable = hit.collider.GetComponent<Breakable>();
        }
        else if (hit.collider.GetComponent<Being>() && !hit.collider.CompareTag("Player") && distanceToBeing <= _characterStats.ShootRange && _playerController.IsAlive)
        {
            allVantagePointsRayCast = false;
            vantagePointIndex = -1;
            _animator.ResetTrigger("run");
            _currentState = JalaliStates.SHOOT;
            InitNewState("shoot", false);
            _previousState = _currentState;
            currentTargetBreakable = null;

            currentVisibleBeing = hit.collider.GetComponent<Being>();

            Debug.Log("Blocked by a being");
        }
        else if(!allVantagePointsRayCast && _playerController.IsAlive){
                
                _muzzleFlash.SetActive(false);
                currentTargetBreakable = null;

                for (int i = 0; i < jalaliVantagePoints.Count; i++)
                {
                    RaycastHit vantageHit;

                    Vector3 vantagePos = jalaliVantagePoints[i].position + transform.up * _sightHeightMultiplier;
                    Vector3 vantageDirection = new Vector3(_player.transform.position.x, 1f, _player.transform.position.z) - vantagePos;

                    Debug.DrawRay(vantagePos, vantageDirection, Color.green, 4f);

                    if (Physics.Raycast(vantagePos, vantageDirection, out vantageHit, Mathf.Infinity))
                    {
                        if (vantageHit.collider.CompareTag("Player"))
                        {
                            if (vantagePointIndex == -1)
                                vantagePointIndex = i;
                            else
                            {
                                float prevDist = Vector3.Distance(jalaliVantagePoints[vantagePointIndex].position, _player.transform.position);
                                float curDist = Vector3.Distance(jalaliVantagePoints[i].position, _player.transform.position);

                                vantagePointIndex = curDist < prevDist ? i : vantagePointIndex;
                            }
                        }
                    }

                }

                allVantagePointsRayCast = true;

                if (vantagePointIndex > -1)
                {
                    _currentState = JalaliStates.RUN;
                    InitNewState("run", false);
                    _previousState = _currentState;
                }
        }
        else
        {
            if (_navMeshAgent.isActiveAndEnabled)
            {
                if (_navMeshAgent.remainingDistance <= 1f)
                {
                    _muzzleFlash.SetActive(false);

                    //_gun.transform.localPosition = _normalTransform.Position;
                    //_gun.transform.localRotation = Quaternion.Euler(_normalTransform.Rotation);

                    _currentState = JalaliStates.NONE;
                    InitNewState("idle", false);
                    _previousState = _currentState;
                    allVantagePointsRayCast = false;
                    vantagePointIndex = -1;
                }
            }
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

                _gun.transform.localPosition = _runTransform.Position;
                _gun.transform.localRotation = Quaternion.Euler(_runTransform.Rotation);

                if(vantagePointIndex>-1)
                    _navMeshAgent.destination = jalaliVantagePoints[vantagePointIndex].position;
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
