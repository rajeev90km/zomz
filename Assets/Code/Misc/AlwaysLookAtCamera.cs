using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour {

    [SerializeField]
    private Vector3 _rotationOffset;

	void Start () {
		
	}

	void Update () {
        Vector3 relativePos = Camera.main.transform.position - transform.position;
        relativePos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(_rotationOffset);
        transform.rotation = rotation;
	}
}
