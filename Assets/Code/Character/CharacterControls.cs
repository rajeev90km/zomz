using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControls : MonoBehaviour {

	[SerializeField]
	private float _moveSpeed = 4.0f;

	Vector3 forward,right;

	void Start () 
	{
		forward = Camera.main.transform.forward;
		forward.y = 0;
		forward = Vector3.Normalize (forward);

		right = Quaternion.Euler (new Vector3 (0, 90, 0)) * forward;
	}


	void Move()
	{
		Vector3 direction = new Vector3 (Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
		Vector3 rightMovement = right * _moveSpeed * Time.deltaTime * Input.GetAxis ("Horizontal");
		Vector3 upMovement = forward * _moveSpeed * Time.deltaTime * Input.GetAxis ("Vertical");

		Vector3 heading = Vector3.Normalize (rightMovement + upMovement);

		transform.forward = heading;
		transform.position += rightMovement + upMovement;

	}
	

	void Update () {
		if (Input.anyKey)
			Move();
	}
}
