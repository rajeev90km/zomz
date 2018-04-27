using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZomzActionSystem : MonoBehaviour {

	[SerializeField]
	private CharacterStats _characterStats;

	Vector3 forward,right;

	private bool _isSelected = false;
	public bool IsSelected
	{
		get { return _isSelected; }	
		set { _isSelected = value; }
	}

	[SerializeField]
	private float _movementSpeed = 2f;

	private Animator _animator;	
	public Animator Animator
	{
		get{ return _animator; }	
	}

	void OnEnable () 
	{
		_animator = GetComponent<Animator> ();

		forward = Camera.main.transform.forward;
		forward.y = 0;
		forward = Vector3.Normalize (forward);

		right = Quaternion.Euler (new Vector3 (0, 90, 0)) * forward;
	}

	void Update () 
	{
		if (_isSelected)
		{
			Vector3 rightMovement = right * _movementSpeed * Time.deltaTime * Input.GetAxis ("Horizontal");
			Vector3 upMovement = forward * _movementSpeed * Time.deltaTime * Input.GetAxis ("Vertical");

			Vector3 heading = Vector3.Normalize (rightMovement + upMovement);

			if (heading != Vector3.zero)
			{
				transform.forward = heading;
				transform.position += rightMovement + upMovement;
				_animator.SetFloat ("speed", _movementSpeed);
			} else
			{
				_animator.SetFloat ("speed", 0);
			}
		
			//Show attack sphere
			if (Input.GetKeyDown (KeyCode.Alpha1))
			{
				//Set UI Attack State
			}
		}

	}

}
