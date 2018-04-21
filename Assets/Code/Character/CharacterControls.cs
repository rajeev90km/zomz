using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Animator))]
public class CharacterControls : MonoBehaviour {

	[SerializeField]
	private CharacterStats _characterStats;

	[SerializeField]
	private Transform _eyes;

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
		_characterStats.Health = _characterStats.InitHealth;

		forward = Camera.main.transform.forward;
		forward.y = 0;
		forward = Vector3.Normalize (forward);

		right = Quaternion.Euler (new Vector3 (0, 90, 0)) * forward;

		_animator = GetComponent<Animator> ();
	}


	public void Attack()
	{
		if (_isAlive)
		{
			if (_attackCoroutine != null)
			{
				StopCoroutine (_attackCoroutine);
				_attackCoroutine = null;
			}

			_attackCoroutine = StartCoroutine (BeginAttack ());
		}
	}

	public void Hurt(float pDamage = 0.0f)
	{
		if (_isAlive)
		{
			if (_characterStats.Health - pDamage > 0)
				_characterStats.Health -= pDamage;
			else
				_characterStats.Health = 0;

			if (_characterStats.Health > 0 && !_isAttacking)
			{
				_animator.SetTrigger (_hurtAnimations [Random.Range (0, _hurtAnimations.Length)]);
			} 
			else if (_characterStats.Health <= 0)
			{
				Die ();
			}
		}
	}


	private IEnumerator BeginAttack()
	{
		_isAttacking = true;
		_canAttack = false;
		_animator.SetTrigger (_attackAnimations[Random.Range(0,_attackAnimations.Length)]);

		GameObject closestEnemy = GetClosestObject ();

		if (closestEnemy)
		{
			AIStateController zombieControls = closestEnemy.GetComponent<AIStateController> ();
			if (zombieControls)
				zombieControls.TakeDamage (_characterStats.AttackStrength);
		}

		yield return new WaitForSeconds(_characterStats.AttackRate);
		_canAttack = true;

		yield return new WaitForSeconds(0.6f);
		_isAttacking = false;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere (transform.position, 1f);
	}

	private IEnumerator BeginHurt(float pDamage = 0.0f)
	{
		_isHurting = true;
		yield return null;
		_isHurting = false;
	}

	public void Die()
	{
		_animator.SetTrigger ("die");
		_isAlive = false;
	}

	public GameObject GetClosestObject()
	{
		Collider[] colliders = Physics.OverlapSphere (transform.position, 1f);
		Collider closestCollider = null;

		foreach (Collider hit in colliders) 
		{
			if((hit.GetComponent<Collider>() == transform.GetComponent<Collider>()) || !hit.transform.CompareTag("Enemy"))
			{
				continue;
			}
			if(!closestCollider)
			{
				closestCollider = hit;
			}
			//compares distances
			if(Vector3.Distance(transform.position, hit.transform.position) <= Vector3.Distance(transform.position, closestCollider.transform.position))
			{
				closestCollider = hit;
			}
		}

		if (!closestCollider)
			return null;

		return closestCollider.gameObject;
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
				Die ();
			}

			//Hurt
			if (Input.GetKeyDown (KeyCode.H))
			{
				Hurt ();
			}
		}
	}
}
