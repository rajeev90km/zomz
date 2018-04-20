using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Animator))]
public class CharacterControls : MonoBehaviour {

	[SerializeField]
	private CharacterStats _characterStats;

	private float _speedSmoothTime = 0.1f;
	private float _speedSmoothVelocity;
	private float _currentSpeed;

	[SerializeField]
	private bool _debugMode = false;

	private bool _isAlive = true;
	public bool IsAlive
	{
		get { return _isAlive; }
	}

	Vector3 forward,right;

	private Animator _animator;

	private bool _isAttacking = false;
	private bool _isHurting = false;
	private bool _canAttack = true;

	private Coroutine _attackCoroutine;
	private Coroutine _hurtCoroutine;

	private string[] _attackAnimations = {"attack1","attack2","attack3","attackcombo1","attackcombo2"};
	private string[] _hurtAnimations = {"hurt1","hurt2","hurt3"};


	void Start () 
	{
		forward = Camera.main.transform.forward;
		forward.y = 0;
		forward = Vector3.Normalize (forward);

		right = Quaternion.Euler (new Vector3 (0, 90, 0)) * forward;

		_animator = GetComponent<Animator> ();
	}


	public void Attack()
	{
		if (_attackCoroutine != null)
		{
			StopCoroutine (_attackCoroutine);
			_attackCoroutine = null;
		}

		_attackCoroutine = StartCoroutine (BeginAttack ());
	}

	public void Hurt()
	{
		if (_hurtCoroutine != null)
		{
			StopCoroutine (_hurtCoroutine);
			_hurtCoroutine = null;
		}

		_hurtCoroutine = StartCoroutine (BeginHurt ());
	}


	private IEnumerator BeginAttack()
	{
		_isAttacking = true;
		_canAttack = false;
		_animator.SetTrigger (_attackAnimations[Random.Range(0,_attackAnimations.Length)]);

		yield return new WaitForSeconds(1.2f);
		_canAttack = true;

		yield return new WaitForSeconds(0.6f);
		_isAttacking = false;
	}


	private IEnumerator BeginHurt()
	{
		_isHurting = true;
		_animator.SetTrigger(_hurtAnimations[Random.Range(0,_hurtAnimations.Length)]);
		yield return new WaitForSeconds(1f);
		_isHurting = false;
	}


	void Update () 
	{
		if (_isAlive)
		{
			//Attack
			if (Input.GetKeyDown (KeyCode.Space))
			{
				if (_canAttack)
				{
					Attack ();
				}
			}

			if (!_isAttacking && !_isHurting)
			{
				bool running = Input.GetKey (KeyCode.LeftShift);
				float targetSpeed = ((running) ? _characterStats.RunSpeed : _characterStats.WalkSpeed);
				_currentSpeed = Mathf.SmoothDamp (_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);

				Vector3 rightMovement = right * _currentSpeed * Time.deltaTime * Input.GetAxis ("Horizontal");
				Vector3 upMovement = forward * _currentSpeed * Time.deltaTime * Input.GetAxis ("Vertical");

				Vector3 heading = Vector3.Normalize (rightMovement + upMovement);

				if (heading != Vector3.zero)
				{
					transform.forward = heading;
					transform.position += rightMovement + upMovement;
				}


				float animationSpeedPercent = ((running) ? 1 : 0.5f) * heading.magnitude;

				if (_animator)
					_animator.SetFloat ("speedPercent", animationSpeedPercent, _speedSmoothTime, Time.deltaTime);
			}
		}


		//DEBUG MODE
		if (_debugMode)
		{
			//Die
			if (Input.GetKeyDown (KeyCode.X))
			{
				_animator.SetTrigger ("die");
				_isAlive = false;
			}

			//Hurt
			if (Input.GetKeyDown (KeyCode.H))
			{
				Hurt ();
			}
		}
	}
}
