using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : Interactable {

    [SerializeField]
    private GameObject _explodeFXPrefab;

    [SerializeField]
    private float _explosionRange = 8f;

    [SerializeField]
    private float _explosionStrength = 100f;

    GameObject _explodeFX;

    private int _enemyLayerMask;
    private int _playerLayerMask;
    private int _enemyAndPlayerLayerMask;

    private bool _isParticleHit = false;

	private void Awake()
	{
        _enemyLayerMask = (1 << LayerMask.NameToLayer("Enemy"));
        _playerLayerMask = (1 << LayerMask.NameToLayer("Player"));
        _enemyAndPlayerLayerMask = _enemyLayerMask | _playerLayerMask;
	}

	private void OnCollisionEnter(Collision collision)
	{
        OnCollide(collision);
	}

    private void OnParticleCollision(GameObject other)
    {
        OnParticleCollide(other);
    }


    public override void OnParticleCollide(GameObject pOther)
    {
        base.OnParticleCollide(pOther);

        if (pOther.CompareTag("ParticleHurt"))
        {
            if (!_isParticleHit)
            {
                _isParticleHit = true;

                Explode();
            }
        }
    }

	public override void OnCollide(Collision pCollision)
	{
        base.OnCollide(pCollision);

        ZombieFast zFast = pCollision.gameObject.GetComponent<ZombieFast>();
        ZomzStrong zStrong = pCollision.gameObject.GetComponent<ZomzStrong>();

        if(zFast!=null && zFast.IsCharging)
        {
            Explode();
        }
        else if(zStrong!=null && zStrong.IsAttackEnded)
        {
            Explode();
        }
	}


    void Explode()
    {
        _explodeFX = Instantiate(_explodeFXPrefab);
        _explodeFX.transform.position = transform.position;

        Collider[] _beingsHit = Physics.OverlapSphere(transform.position, _explosionRange, _enemyAndPlayerLayerMask);

        for (int i = 0; i < _beingsHit.Length; i++)
        {
            Being being = _beingsHit[i].GetComponent<Being>();

            if (being != null && being.IsAlive)
            {
                float d = Vector3.Distance(being.transform.position, transform.position);

                if (d <= _explosionRange)
                {
                    float hurtAmount = (_explosionRange - d) / _explosionRange * _explosionStrength;
                    being.StartCoroutine(being.Hurt(hurtAmount));
                }
            }
        }

        Destroy(gameObject);
    }
}
