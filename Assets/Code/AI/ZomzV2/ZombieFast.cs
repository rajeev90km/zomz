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

    private Coroutine _chargeCoroutine;

    private bool _isCharging = false;

	protected override void Awake()
	{
        base.Awake();

        Charge();
	}

    protected void Charge()
    {
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }
        _chargeCoroutine =StartCoroutine(BeginCharge());
    }

    protected IEnumerator BeginCharge()
    {
        _isCharging = true;

        int rand = Random.Range(_minChargeRate, _maxChargeRate);

        float time = 0;

        if (!IsHurting)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = transform.position + transform.forward * _chargeDistance;

            while (time < 1)
            {
                transform.position = Vector3.Lerp(startPos, endPos, time);
                time = time / _timeToCharge + Time.deltaTime;
                yield return null;
            }

            _isCharging = false;
        }

        if (!IsBeingControlled)
        {
            yield return new WaitForSeconds(rand);
            Charge();
        }
       
        yield return null;
    }


	protected override void Update()
	{
        base.Update();


        if(!IsHurting && !IsBeingControlled)
        {
            //int rand = Random.Range(_minChargeTime,_maxChargeTime)
        }
	}
}
