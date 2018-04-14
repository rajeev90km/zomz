using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FovZone : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag("Moveable")) {

			foreach(Transform t in other.transform)
				t.gameObject.layer = 8;
		}
	}

	void OnTriggerExit(Collider other){
		if (other.gameObject.CompareTag("Moveable")) {
			foreach(Transform t in other.transform)
				t.gameObject.layer = 9;
		}
	}
}
