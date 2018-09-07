using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieFast : ZombieBase 
{
    [Header("Charge Characteristics")]
    [SerializeField]
    private float _timeToCharge;

    [SerializeField]
    private int _minChargeRate;

    [SerializeField]
    private int _maxChargeRate;

    [SerializeField]
    private float _chargeDistance;

    [SerializeField]
    private GameObject _chargeFX;

    [SerializeField]
    private float _chargeFXLifetime = 1.5f;

    private Coroutine _chargeCoroutine;

    private bool _isCharging = false;
    public bool IsCharging
    {
        get { return _isCharging; }
    }

    public bool IsFast = true;

    GameObject _chargeFXObj;

	protected override void Awake()
	{
        base.Awake();

        Charge();
	}

    protected void Charge()
    {
        if (_isAlive)
        {
            if (_chargeCoroutine != null)
            {
                StopCoroutine(_chargeCoroutine);
                _chargeCoroutine = null;
            }

            _chargeCoroutine = StartCoroutine(BeginCharge());
        }
    }

    protected IEnumerator BeginCharge()
    {
        if (_isAlive)
        {
            _isCharging = true;

            int rand = Random.Range(_minChargeRate, _maxChargeRate);

            float time = 0;

            if (!IsHurting && !IsAttacking)
            {
                Vector3 startPos = transform.position;
                Vector3 endPos = transform.position + transform.forward * _chargeDistance;

                _chargeFXObj = Instantiate(_chargeFX);
                _chargeFXObj.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

                while (time < 1)
                {
                    transform.position = Vector3.Lerp(startPos, endPos, time);
                    _chargeFXObj.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                    time = time / _timeToCharge + Time.deltaTime;
                    yield return null;
                }

                Destroy(_chargeFXObj, 1f);

                _isCharging = false;
            }

            if (!IsBeingControlled)
            {
                yield return new WaitForSeconds(rand);
                Charge();
            }
        }
       
        yield return null;
    }

	public override void OnZomzModeUnRegister()
	{
        base.OnZomzModeUnRegister();
        IsHurting = false;
        IsAttacking = false;
        Charge();
	}

	public override IEnumerator Attack()
	{
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }

        IsAttacking = true;

        if (!IsBeingControlled)
        {
            finalLayerMask = humanLayerMask | playerLayerMask;
            Being closestBeing = GetClosestBeingToAttack(finalLayerMask, CharacterStats.AttackRange);

            transform.LookAt(closestBeing.transform);

            if (Vector3.Distance(closestBeing.transform.position, transform.position) <= CharacterStats.AttackRange && !IsHurting)
            {
                float time = 0;
                Vector3 startPos = transform.position;
                Vector3 endPos = transform.position + transform.forward * _chargeDistance;

                _chargeFXObj = Instantiate(_chargeFX);
                _chargeFXObj.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

                while (time < 1)
                {
                    transform.position = Vector3.Lerp(startPos, endPos, time);
                    _chargeFXObj.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                    time = time / _timeToCharge + Time.deltaTime;
                    yield return null;
                }

                Destroy(_chargeFXObj, 1f);

                _animator.SetTrigger("idle");
                yield return new WaitForSeconds(CharacterStats.AttackRate);
                IsAttacking = false;

                Charge();
            }
        }
        else
        {
            _animator.SetTrigger("attack");

            _animState = ZombieStates.ATTACK;

            if (ZomzMode.ManaConsumeType == ZomzManaConsumeType.ACTION_BASED)
                _zomzManaAttribute.CurrentValue -= _attackCost;

            finalLayerMask = humanLayerMask | playerLayerMask | zombieLayerMask;

            Being closestEnemy = GetClosestBeingToAttack(finalLayerMask,CharacterStats.AttackRange);

            if (closestEnemy)
                transform.LookAt(closestEnemy.transform);

            float time = 0;
            Vector3 startPos = transform.position;
            Vector3 endPos = transform.position + transform.forward * _chargeDistance;

            _chargeFXObj = Instantiate(_chargeFX);
            _chargeFXObj.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

            _isCharging = true;

            while (time < 1)
            {
                transform.position = Vector3.Lerp(startPos, endPos, time);
                _chargeFXObj.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                time = time / _timeToCharge + Time.deltaTime;
                yield return null;
            }

            _isCharging = false;

            Destroy(_chargeFXObj, 1f);

            _animator.SetTrigger("idle");
            yield return new WaitForSeconds(CharacterStats.AttackRate);
            IsAttacking = false;
        }

        yield return null;
	}

    protected override void DieState()
    {
        base.DieState();

        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }
    }

	private void OnCollisionEnter(Collision pOther)
	{
        if (!IsBeingControlled)
        {
            if (pOther.gameObject.CompareTag("Player") || pOther.gameObject.CompareTag("Human"))
            {
                Being otherBeing = pOther.gameObject.GetComponent<Being>();

                if(otherBeing && _isCharging)
                    StartCoroutine(otherBeing.Hurt(CharacterStats.AttackStrength));
            }
        }
        else
        {
            if (pOther.gameObject.CompareTag("Player") || pOther.gameObject.CompareTag("Human") || pOther.gameObject.CompareTag("Enemy"))
            {
                Being otherBeing = pOther.gameObject.GetComponent<Being>();

                if (otherBeing && _isCharging)
                    StartCoroutine(otherBeing.Hurt(CharacterStats.AttackStrength));
            }
        }
	}

	#region ZomzMode

	public override void StartZomzMode()
	{
        base.StartZomzMode();

        if(IsAlive)
        {
            if(_chargeCoroutine!=null)
            {
                StopCoroutine(_chargeCoroutine);
                _chargeCoroutine = null;
            }
        }
	}

	#endregion

	protected override void Update()
	{
        base.Update();


        if(!IsHurting && !IsBeingControlled)
        {
            //int rand = Random.Range(_minChargeTime,_maxChargeTime)
        }
	}
}
