using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerWaveAdvancer : MonoBehaviour {

    private bool _hasTriggered = false;

    [SerializeField]
    private bool _triggerOnExit = false;

    [SerializeField]
    private bool _delayTrigger = false;

    [DrawIf("_delayTrigger", true)]
    [SerializeField]
    private float _delayTime = 3f;


    [SerializeField]
    private GameEvent _triggerNextWaveEvent;

    public IEnumerator TriggerNextWaveEvent()
    {
        if(!_hasTriggered)
        {
            if(_triggerNextWaveEvent!=null)
            {
                _hasTriggered = true;

                if(_delayTrigger == true)
                {
                    yield return new WaitForSeconds(_delayTime);
                }
                _triggerNextWaveEvent.Raise();
                yield return null;
            }
        }
    }

	private void OnTriggerEnter(Collider other)
	{
        if(!_triggerOnExit)
        {
            if (!_hasTriggered)
            {
                if (other.CompareTag("Player"))
                {
                    StartCoroutine(TriggerNextWaveEvent());
                }
            }
        }
	}

    private void OnTriggerExit(Collider other)
    {
        if (_triggerOnExit)
        {
            if (!_hasTriggered)
            {
                if (other.CompareTag("Player"))
                {
                    StartCoroutine(TriggerNextWaveEvent());
                }
            }
        }
    }



}
