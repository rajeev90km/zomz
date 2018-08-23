using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZomzStrong : ZombieBase {

    [Header("Strong Zombie Characteristics")]
    [SerializeField]
    private GameObject _stompFx;

    private GameObject stompFxObj;

    private bool _isAttackStarted = false;

    private int _enemyLayerMask;

	protected override void Awake()
	{
        base.Awake();

        _enemyLayerMask = (1 << LayerMask.NameToLayer("Enemy"));
	}

	public override void OnZomzModeUnRegister()
    {
        base.OnZomzModeUnRegister();
        IsHurting = false;
        IsAttacking = false;
    }

    public override IEnumerator Hurt(float pDamage = 0.0f)
    {
        if (IsAlive && !_isAttackStarted)
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

                IsHurting = true;
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

                yield return new WaitForSeconds(CharacterStats.HurtRate);

                IsHurting = false;
                IsAttacking = false;

                if (_currentHealth <= 0)
                {
                    DieState();
                }
            }
        }

        yield return null;
    }


    public override IEnumerator Attack()
    {
        if (!IsAttacking)
        {
            IsAttacking = true;

            if (!IsBeingControlled)
                transform.LookAt(_player.transform);

            Vector3 startPos;
            Vector3 endPos;
            if (!IsBeingControlled)
            {
                yield return new WaitForSeconds(0.5f);

                startPos = transform.position;
                endPos = transform.position + new Vector3(0, 3, 0);

                float time = 0;

                _isAttackStarted = true;

                yield return new WaitForSeconds(0.4f);

                while (time < 0.5f)
                {
                    transform.position = Vector3.Lerp(startPos, endPos, time);
                    time = time + Time.deltaTime;
                    yield return null;
                }
                transform.position = endPos;

                startPos = transform.position;
                endPos = new Vector3(transform.position.x, 0, transform.position.z);

                time = 1;
                transform.position = endPos;

                _isAttackStarted = false;

                Vector3 fxPos = transform.position + transform.forward * 2f;

                stompFxObj = Instantiate(_stompFx);
                stompFxObj.transform.position = new Vector3(fxPos.x, 0f, fxPos.z);

                if (Vector3.Distance(_player.transform.position, transform.position) <= CharacterStats.StompRange && !IsHurting)
                {
                    StartCoroutine(_playerController.Hurt(CharacterStats.AttackStrength));
                }
            }
            else
            {
                _animator.SetTrigger("attack");

                yield return new WaitForSeconds(0.5f);

                startPos = transform.position;
                endPos = transform.position + new Vector3(0, 3, 0);

                float time = 0;

                _isAttackStarted = true;

                yield return new WaitForSeconds(0.4f);

                while (time < 0.5f)
                {
                    transform.position = Vector3.Lerp(startPos, endPos, time);
                    time = time + Time.deltaTime;
                    yield return null;
                }
                transform.position = endPos;

                startPos = transform.position;
                endPos = new Vector3(transform.position.x, 0, transform.position.z);


                _isAttackStarted = false;

                time = 1;
                transform.position = endPos;

                _isAttackStarted = false;

                Vector3 fxPos = transform.position + transform.forward * 2f;

                stompFxObj = Instantiate(_stompFx);
                stompFxObj.transform.position = new Vector3(fxPos.x, 0f, fxPos.z);

                Collider[] _zombiesHit = Physics.OverlapSphere(transform.position, CharacterStats.StompRange, _enemyLayerMask);

                for (int i = 0; i < _zombiesHit.Length; i++)
                {
                    ZombieBase zombieBase = _zombiesHit[i].GetComponent<ZombieBase>();

                    if (zombieBase != null && zombieBase.transform != transform)
                    {
                        if (Vector3.Distance(zombieBase.transform.position, transform.position) <= CharacterStats.StompRange && !IsHurting)
                        {
                            zombieBase.StartCoroutine(zombieBase.Hurt(CharacterStats.AttackStrength));
                        }
                    }
                }
            }

            yield return new WaitForSeconds(CharacterStats.AttackRate / 2);
            IsAttacking = false;

        }
        yield return null;
    }

}
