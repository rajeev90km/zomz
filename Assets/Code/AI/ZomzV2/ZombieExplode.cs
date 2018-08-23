using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieExplode : ZombieBase 
{
    [SerializeField]
    private GameObject _explosionFXPrefab;

    private int _enemyLayerMask;
    private int _playerLayerMask;
    private int _enemyAndPlayerLayerMask;

    GameObject _explosionFXObj;

    ZomzController _zomzController;

	protected override void Awake()
	{
        base.Awake();

        _enemyLayerMask = (1 << LayerMask.NameToLayer("Enemy"));
        _playerLayerMask = (1 << LayerMask.NameToLayer("Player"));

        _zomzController = GameObject.FindWithTag("Player").GetComponent<ZomzController>();
	}

	public override void OnZomzModeUnRegister()
	{
        base.OnZomzModeUnRegister();
        IsHurting = false;
        IsAttacking = false;
	}


    public override IEnumerator Hurt(float pDamage = 0)
	{
        IsHurting = true;
        IsAttacking = false;

        if(_attackCoroutine==null)
        {
            _animator.SetTrigger("attack");
            StartCoroutine(Attack());
        }

        yield return null;
	}


	public override IEnumerator Attack()
    {
        if (IsAlive && !IsAttacking)
        {
            if (IsBeingControlled)
                _animator.SetTrigger("attack");

            IsAttacking = true;

            yield return new WaitForSeconds(CharacterStats.AttackRate);

            Collider[] _zombiesHit = Physics.OverlapSphere(transform.position, CharacterStats.ExplosionRange, _enemyLayerMask);
            Collider[] _playerHit = Physics.OverlapSphere(transform.position, CharacterStats.ExplosionRange, _playerLayerMask);

            for (int i = 0; i < _zombiesHit.Length; i++)
            {
                ZombieBase zombieBase = _zombiesHit[i].GetComponent<ZombieBase>();

                if (zombieBase != null && zombieBase.transform != transform)
                {
                    float d = Vector3.Distance(zombieBase.transform.position, transform.position);

                    if (d <= CharacterStats.ExplosionRange)
                    {
                        zombieBase.StartCoroutine(zombieBase.Hurt( (CharacterStats.ExplosionRange - d )/CharacterStats.ExplosionRange * CharacterStats.AttackStrength));
                    }
                }
            }

            for (int i = 0; i < _playerHit.Length; i++)
            {
                CharacterControls player = _playerHit[i].GetComponent<CharacterControls>();

                if (player != null)
                {
                    float d = Vector3.Distance(player.transform.position, transform.position);
                    if (d <= CharacterStats.ExplosionRange)
                    {
                        player.StartCoroutine(player.Hurt((CharacterStats.ExplosionRange - d) / CharacterStats.ExplosionRange * CharacterStats.AttackStrength));
                    }
                }
            }

            IsAttacking = false;


            if(_explosionFXPrefab!=null)
            {
                _explosionFXObj = Instantiate(_explosionFXPrefab);
                _explosionFXObj.transform.position = transform.position;
                IsAlive = false;
            }

            yield return new WaitForSeconds(0.3f);

            if (!IsHurting)
                _zomzController.UnregisterZomzMode();
            Destroy(gameObject);


        }

        yield return null;
    }
}
