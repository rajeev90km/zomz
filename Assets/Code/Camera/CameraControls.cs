using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangedObject
{
    public Renderer renderer;
    public Material[] originalMaterials;


    public ChangedObject(Renderer renderer, Material[] materials)
    {
        this.renderer = renderer;
        originalMaterials = renderer.sharedMaterials;
        renderer.materials = materials;
    }

}

public class CameraControls : MonoBehaviour
{

    [SerializeField]
    private Transform _cameraTopPosition;

    [SerializeField]
    private Transform _cameraBottomPosition;

    [SerializeField]
    private Transform _targetTransform;

    [SerializeField]
    private Transform _rayCastPlayerTransform;

    [SerializeField]
    [Range(0f, 1f)]
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

    private float _zoomValue = 0.5f;

    void Start()
    {
        
        if (_mask != null)
            _mask.SetActive(false);

        if (_targetTransform != null)
            _playerCharacter = _targetTransform.gameObject.GetComponent<CharacterControls>();

        //// Create 'S' shaped curve to adjust pitch
        //// Varies from 0 (looking forward) at 0, to 90 (looking straight down) at 1
        //_pitchCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 90.0f);

        //// Create exponential shaped curve to adjust distance
        //// So zoom control will be more accurate at closer distances, and more coarse further away
        //Keyframe[] ks = new Keyframe[2];
        //// At zoom=0, offset by 0.5 units
        //ks[0] = new Keyframe(0, 0.5f);
        //ks[0].outTangent = 0;
        //// At zoom=1, offset by 60 units
        //ks[1] = new Keyframe(1, 60);
        //ks[1].inTangent = 90;
        //_distanceCurve = new AnimationCurve(ks);

        transform.position = Vector3.Lerp(_cameraTopPosition.position, _cameraBottomPosition.position, _zoomValue);
        transform.rotation = Quaternion.Slerp(_cameraTopPosition.rotation, _cameraBottomPosition.rotation, _zoomValue);

        _cameraOffset = transform.position - _targetTransform.position;

    }


    void Update()
    {
        //raycast
        //      if (_rayCastPlayerTransform != null) {
        //          Vector3 direction = _rayCastPlayerTransform.position - transform.position;
        //
        //          if (Physics.Raycast(transform.position, direction, out _hit)) {
        //              if (!_hit.collider.gameObject.CompareTag ("Player")) {
        //
        //                  if (_mask != null)
        //                  {
        //                      _mask.SetActive (true);
        //                      _mask.transform.position = _hit.point;
        //                  }
        //
        //
        //                  Renderer hitRenderer = _hit.transform.GetComponent<Renderer>();
        //                  if (hitRenderer) {
        //                      if (changedObject != null) {
        //                          if (changedObject.renderer == hitRenderer)
        //                              return;
        //                          else
        //                              changedObject.renderer.materials = changedObject.originalMaterials;
        //                      }
        //                      changedObject = new ChangedObject (hitRenderer, hitRenderer.sharedMaterials);
        //                  }
        //
        //                  r = _hit.collider.gameObject.GetComponent<Renderer> ();
        //                  oldRenderer = r;
        //
        //                  for(int i=0;i<r.materials.Length;i++)
        //                      r.materials[i].renderQueue = 3000;
        //
        //
        //
        //              } else {
        //                  if (changedObject != null) {
        //                      changedObject.renderer.materials = changedObject.originalMaterials;
        //                      changedObject = null;
        //                  }
        //
        //                  if (_mask != null)
        //                      _mask.SetActive (false);
        //              }
        //          }
        //      }
    }

    void LateUpdate()
    {

        if (_targetTransform != null)
        {

            if (_playerCharacter != null)
                _playerCharacter.ResetDirectionVectors();

            _zoomValue -= Input.GetAxis("Mouse ScrollWheel") * _zoomSensitivity;


            if (_zoomValue > 1)
                _zoomValue = 1;

            if (_zoomValue < 0)
                _zoomValue = 0;


            Vector3 finalPos = Vector3.Lerp(_cameraTopPosition.position, _cameraBottomPosition.position, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, _zoomValue)));
            Quaternion finalRot = Quaternion.Slerp(_cameraTopPosition.rotation, _cameraBottomPosition.rotation, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, _zoomValue)));
            //float fov = Camera.main.fieldOfView;
            //fov += Input.GetAxis("Mouse ScrollWheel") * _zoomSensitivity;
            //fov = Mathf.Clamp(fov, _minFov, _maxFov);
            //Camera.main.fieldOfView = fov;

            transform.position = Vector3.Lerp(transform.position, finalPos, _smoothnessFactor);
            transform.rotation = Quaternion.Lerp(transform.rotation, finalRot, _smoothnessFactor);

            //_cameraOffset = transform.position - _targetTransform.position;     

            //if(Input.GetMouseButton(0))
            //    _cameraOffset = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * _rotateSpeed, Vector3.up) * _cameraOffset;

            //Vector3 newPos = _targetTransform.position + _cameraOffset;
            //transform.position = Vector3.Slerp(transform.position, newPos, _smoothnessFactor);
            //transform.LookAt(_targetTransform);




        }
    }
}
