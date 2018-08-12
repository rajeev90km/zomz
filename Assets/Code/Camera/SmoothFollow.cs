using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour {

    [SerializeField]
    private Transform _targetTransform;

    [SerializeField]
    private Transform _defaultFollowTransform;

    [SerializeField]
    [Range(0f, 1f)]
    private float _smoothnessFactor;

    [SerializeField]
    private float _rotateSpeed = 1000f;

    [SerializeField]
    private ZomzData _zomzMode;

    private Vector3 _offset;
    private Vector3 _offsetWithoutY;

	void Start () 
    {
        _targetTransform = _defaultFollowTransform;
        _offset	= transform.position - _targetTransform.position;     
	}

    public void OnZomzRegister()
    {
        if(_zomzMode.CurrentSelectedZombie)
        {
            _targetTransform = _zomzMode.CurrentSelectedZombie.transform;
            _smoothnessFactor = 0.2f;

            StartCoroutine(ResetSmoothness());
        }
    }

    public void OnZomzUnregister()
    {
        _targetTransform = _defaultFollowTransform;
        _smoothnessFactor = 0.2f;

        StartCoroutine(ResetSmoothness());
    }

    IEnumerator ResetSmoothness(){
        yield return new WaitForSeconds(1f);
        _smoothnessFactor = 1;
    }

	// Update is called once per frame
	void LateUpdate () 
    {
        if (Input.GetMouseButton(0))
        {
            _offset = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * _rotateSpeed, Vector3.up) * _offset;
        }
        
        Vector3 newPos = _targetTransform.position + _offset;
        transform.position = Vector3.Slerp(transform.position, newPos, _smoothnessFactor);

        _offsetWithoutY = transform.position - _targetTransform.position;
        _offsetWithoutY.y = 0;

        if (Input.GetMouseButton(0))
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * _rotateSpeed);
        //transform.rotation = Quaternion.LookRotation(_offsetWithoutY);
        //transform.LookAt(_targetTransform);
		
	}
}
