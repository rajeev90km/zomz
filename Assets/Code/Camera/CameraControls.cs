using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangedObject {
	public Renderer renderer;
	public Material[] originalMaterials;


	public ChangedObject (Renderer renderer, Material[] materials) {
		this.renderer = renderer;
		originalMaterials = renderer.sharedMaterials;
		renderer.materials = materials;
	}

}

public class CameraControls : MonoBehaviour {

	[SerializeField]
	private Transform _targetTransform;

	[SerializeField]
	private Transform _rayCastPlayerTransform;

	[SerializeField]
	private float _rotateSpeed = 1000f;

	[SerializeField]
	[Range(0f,1f)]
	private float _smoothnessFactor;

	private Vector3 _cameraOffset;

	[SerializeField]
	private GameObject _mask;

    [SerializeField]
    private float _minFov = 45f;

    [SerializeField]
    private float _maxFov = 90f;

    [SerializeField]
    private float _zoomSensitivity = 10f;

	RaycastHit _hit;
	Renderer oldRenderer;
	Renderer r;
	ChangedObject changedObject;
    CharacterControls _playerCharacter;

	void Start () 
	{
		_cameraOffset = transform.position - _targetTransform.position;		

		if(_mask!=null)
			_mask.SetActive (false);

        if (_targetTransform != null)
            _playerCharacter = _targetTransform.gameObject.GetComponent<CharacterControls>();


	}


	void Update(){
		//raycast
//		if (_rayCastPlayerTransform != null) {
//			Vector3 direction = _rayCastPlayerTransform.position - transform.position;
//
//			if (Physics.Raycast(transform.position, direction, out _hit)) {
//				if (!_hit.collider.gameObject.CompareTag ("Player")) {
//
//					if (_mask != null)
//					{
//						_mask.SetActive (true);
//						_mask.transform.position = _hit.point;
//					}
//
//
//					Renderer hitRenderer = _hit.transform.GetComponent<Renderer>();
//					if (hitRenderer) {
//						if (changedObject != null) {
//							if (changedObject.renderer == hitRenderer)
//								return;
//							else
//								changedObject.renderer.materials = changedObject.originalMaterials;
//						}
//						changedObject = new ChangedObject (hitRenderer, hitRenderer.sharedMaterials);
//					}
//
//					r = _hit.collider.gameObject.GetComponent<Renderer> ();
//					oldRenderer = r;
//
//					for(int i=0;i<r.materials.Length;i++)
//						r.materials[i].renderQueue = 3000;
//
//
//
//				} else {
//					if (changedObject != null) {
//						changedObject.renderer.materials = changedObject.originalMaterials;
//						changedObject = null;
//					}
//
//					if (_mask != null)
//						_mask.SetActive (false);
//				}
//			}
//		}
	}

	void LateUpdate () {

		if (_targetTransform != null) {

            if(_playerCharacter!=null)
                _playerCharacter.ResetDirectionVectors();

            float fov = Camera.main.fieldOfView;
            fov += Input.GetAxis("Mouse ScrollWheel") * _zoomSensitivity;
            fov = Mathf.Clamp(fov, _minFov, _maxFov);
            Camera.main.fieldOfView = fov;

			if (Input.GetMouseButton (0)) {
				transform.RotateAround (_targetTransform.position, Vector3.up, Input.GetAxis ("Mouse X") * _rotateSpeed * Time.deltaTime);
				_cameraOffset = transform.position - _targetTransform.position;		
			} else {			
				Vector3 newPos = _targetTransform.position + _cameraOffset;
				transform.position = Vector3.Slerp (transform.position, newPos, _smoothnessFactor);
				transform.LookAt (_targetTransform);
			}
		}
	}
}
