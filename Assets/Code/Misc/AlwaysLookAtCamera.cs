﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour {
	
	void Start () {
		
	}

	void Update () {
		transform.LookAt (Camera.main.transform, -Vector3.up);
	}
}
