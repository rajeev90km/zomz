using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieScream : ZombieBase {

    [SerializeField]
    private GameObject _model2;

    [SerializeField]
    private GameObject _model3;

    private Renderer _model2Renderer;
    private Renderer _model3Renderer;

    [Header("Scream Zombie Config")]
    [SerializeField]
    private GameObject _screamFXPrefab;

    private GameObject _screamFXObj;

    private bool _isFleePointFound = false;

    private bool _isFleeing = false;
    public bool IsFleeing
    {
        get { return _isFleeing; }
        set { _isFleeing = value; }
    }

	protected override void Awake()
	{
        base.Awake();

        _model2Renderer = _model2.GetComponent<Renderer>();
        _model3Renderer = _model3.GetComponent<Renderer>();

	}

	public override void EndZomzMode()
    {
        _modelRenderer.material = _defaultMaterial;
        _model2Renderer.material = _defaultMaterial;
        _model3Renderer.material = _defaultMaterial;
    }

    public override void OnZomzModeAffected()
    {
        _modelRenderer.material = _zomzModeMaterial;
        _model2Renderer.material = _zomzModeMaterial;
        _model3Renderer.material = _zomzModeMaterial;
    }

    public override void OnZomzModeRegister()
    {
        IsBeingControlled = true;
        _modelRenderer.material = _zomzModeMaterial;
        _model2Renderer.material = _zomzModeMaterial;
        _model3Renderer.material = _zomzModeMaterial;
        ResetDirectionVectors();
    }

    public override void OnZomzModeUnRegister()
    {
        IsBeingControlled = false;
        _modelRenderer.material = _zomzModeMaterial;
        _model2Renderer.material = _zomzModeMaterial;
        _model3Renderer.material = _zomzModeMaterial;
    }

    public override IEnumerator Hurt(float pDamage = 0.0f)
    {
        if (IsAlive)
        {
            if (pDamage > 0)
            {
                if (_currentHealth - pDamage > 0)
                    _currentHealth -= pDamage;
                else
                    _currentHealth = 0;

                if (_zombieHealthBar)
                    _zombieHealthBar.fillAmount = _currentHealth / CharacterStats.Health;

                if (_hurtFx != null)
                    Instantiate(_hurtFx, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity);

                if (_currentHealth <= 0)
                {
                    IsAlive = false;
                    IsAttacking = false;
                    IsHurting = false;
                    DieState();
                }
            }
        }

        yield return null;
    }

    public override IEnumerator Attack()
    {
        if (IsAlive && !IsAttacking)
        {
            if (IsBeingControlled)
                _animator.SetTrigger("attack");
            else
                transform.LookAt(_player.transform);

            IsAttacking = true;

            GameObject[] allZombies = GameObject.FindGameObjectsWithTag("Enemy");

            yield return new WaitForSeconds(0.5f);

            if (_screamFXPrefab != null)
            {
                _screamFXObj = Instantiate(_screamFXPrefab);
                _screamFXObj.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
            }

            //query all zombies and draw them to current position
            for (int i = 0; i < allZombies.Length;i++)
            {
                //except itself
                if(allZombies[i]!=gameObject)
                {
                    ZombieBase zombieBase = allZombies[i].GetComponent<ZombieBase>();
                    if(zombieBase.IsAlive)
                    {
                        zombieBase.IsChaseOverridden = true;
                        zombieBase.OverridenChasePosition = new Vector3(transform.position.x,0,transform.position.z);
                    }
                }
            }

            yield return new WaitForSeconds(CharacterStats.AttackRate-0.5f);

            IsAttacking = false;
            _isFleeing = true;
        }

        yield return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, CharacterStats.LookRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, CharacterStats.AttackRange);
    }


    protected virtual void FleeState()
    {
        if (_isAlive && !IsBeingControlled)
        {
            if (_navMeshAgent.isActiveAndEnabled)
            {
                if(!_isFleePointFound)
                {
                    GetNextWayPoint();
                    _isFleePointFound = true;
                }

                _navMeshAgent.speed = CharacterStats.RunSpeed;

                _navMeshAgent.destination = _wayPoints[_nextWayPoint].position;
                _navMeshAgent.isStopped = false;

                if (Vector3.Distance(transform.position,_wayPoints[_nextWayPoint].position) <= 1f)
                {
                    _isFleeing = false;
                    _isFleePointFound = false;
                }
            }
        }
    }


    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected override void ExecuteAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

        //Transition to CHASE mode if close enough to the player
        if (_isFleeing)
        {
            _currentState = ZombieStates.FLEE;
            InitNewState("run");
            _previousState = _currentState;
        }
        else if (_playerController.IsAlive && (((distanceToPlayer < CharacterStats.LookRange) && (distanceToPlayer > CharacterStats.AttackRange)) || (!IsAttacking && _previousState == ZombieStates.ATTACK && distanceToPlayer > CharacterStats.AttackRange)))
        {
            _currentState = ZombieStates.CHASE;
            InitNewState("run", false);
            _previousState = _currentState;
        }
        //Transition to ATTACK if in attack range
        else if (_playerController.IsAlive && (distanceToPlayer <= CharacterStats.AttackRange))
        {
            _currentState = ZombieStates.ATTACK;
            InitNewState("attack", true);
            _previousState = _currentState;
        }
        //Transition to PATROL if it doesn't meet any of the criteria
        else if (!IsAttacking && distanceToPlayer > CharacterStats.LookRange && _previousState == ZombieStates.CHASE)
        {
            _currentState = ZombieStates.PATROL;
            InitNewState("walk");
            _previousState = _currentState;
        }
        else if (!_playerController.IsAlive)
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


        switch (_currentState)
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
            case ZombieStates.FLEE:
                FleeState();
                break;
            default:
                break;
        }
    }

}
