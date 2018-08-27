using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieVomit : ZombieBase 
{
    [Header("Vomit Zombie Config")]
    [SerializeField]
    private Transform _fxParent;

    [SerializeField]
    private Transform _vomitSpawnPosition;

    [SerializeField]
    private GameObject _vomitFXPrefab;

    private int _enemyLayerMask;
    private int _playerLayerMask;

    GameObject _vomitFX;

    protected override void Awake()
    {
        base.Awake();

        _enemyLayerMask = (1 << LayerMask.NameToLayer("Enemy"));
        _playerLayerMask = (1 << LayerMask.NameToLayer("Player"));
    }

    public override IEnumerator Hurt(float pDamage = 0.0f)
    {
        if (IsAlive )
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
                    DieState();
                    _navMeshAgent.enabled = false;
                    transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                }
            }
        }

        yield return null;
    }

	public override IEnumerator Attack()
    {
        if (IsAlive && !IsAttacking)
        { 
            if(!IsBeingControlled)
                transform.LookAt(_player.transform);

            if (IsBeingControlled)
                _animator.SetTrigger("attack");

            IsAttacking = true;

            yield return new WaitForSeconds(0.8f);
            if (_vomitFXPrefab != null)
            {
                _vomitFX = Instantiate(_vomitFXPrefab, _fxParent);
                _vomitFX.transform.position = _vomitSpawnPosition.position;
            }

            yield return new WaitForSeconds(CharacterStats.AttackRate - 0.8f);

            IsAttacking = false;
        }

        yield return null;
    }
}
