using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class CharacterControls : Being
{

    [SerializeField]
    private GameData _gameData;

    [SerializeField]
    private CharacterStats _characterStats;
    public CharacterStats CharacterStats
    {
        get { return _characterStats; }
    }

    [SerializeField]
    private Inventory _inventory;

    [SerializeField]
    private int _attackModifier;
    public int AttackModifier{
        get { return _attackModifier; }
        set { _attackModifier = value; }
    }

    private InventoryItem _currentWeapon;
    public InventoryItem CurrentWeapon {
        get { return _currentWeapon; }
        set { _currentWeapon = value; }
    }

    [SerializeField]
    private Transform _eyes;
    public Transform Eyes
    {
        get { return _eyes; }
    }

    public float _currentHealth;

    private float _speedSmoothTime = 0.1f;
    private float _speedSmoothVelocity;
    private float _currentSpeed;

    [SerializeField]
    private bool _debugMode = false;

    Vector3 forward, right;

    private Animator _animator;
    private ZomzController _zomzControls;

    private bool _isAttacking = false;
    private bool _isHurting = false;
    private bool _canAttack = true;
    private bool _isDiving = false;

    private Coroutine _attackCoroutine;
    private Coroutine _hurtCoroutine;

    private string[] _attackAnimations = { "attack1", "attack2" };
    private string[] _hurtAnimations = { "hurt1"};
    private string _dodgeAnimation = "roll";

    [Header("FX")]
    [SerializeField]
    private GameObject _hurtFX;



    void Start()
    {
        _zomzControls = GetComponent<ZomzController>();
        _currentHealth = _characterStats.Health;

        _animator = GetComponent<Animator>();

        ResetDirectionVectors();
    }

    public void ResetDirectionVectors()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }


    public void BeginAttack()
	{
		if (_isAlive)
		{
			if (_attackCoroutine != null)
			{
				StopCoroutine (_attackCoroutine);
				_attackCoroutine = null;
			}

			_attackCoroutine = StartCoroutine (Attack ());
		}
	}

	public override IEnumerator Hurt(float pDamage = 0.0f)
	{
		if (_isAlive)
        {
            _isHurting = true;

            if (_currentHealth - pDamage > 0)
                _currentHealth -= pDamage;
            else
                _currentHealth = 0;

            if (_hurtFX != null)
                Instantiate(_hurtFX, _eyes.transform.position, Quaternion.identity);

            if (_currentHealth > 0 && !_isAttacking)
            {
                _animator.SetTrigger(_hurtAnimations[Random.Range(0, _hurtAnimations.Length)]);
            }
            else if (_currentHealth <= 0)
            {
                Die();
            }

            yield return new WaitForSeconds(1f);
            _isHurting = false;

        }

		yield return null;
	}


    public override IEnumerator Attack()
    {
        _isAttacking = true;
        _canAttack = false;
        _animator.SetTrigger(_attackAnimations[Random.Range(0, _attackAnimations.Length)]);

        GameObject closestEnemy = GetClosestObject();

        yield return new WaitForSeconds(_characterStats.AttackRate / 2);

        if (closestEnemy)
        {
            transform.LookAt(closestEnemy.transform);
            ZombieBase zombieControls = closestEnemy.GetComponent<ZombieBase>();
            if (zombieControls)
                StartCoroutine(zombieControls.Hurt(_characterStats.AttackStrength + _attackModifier));
        }

        yield return new WaitForSeconds(_characterStats.AttackRate/2);
        _canAttack = true;

        //Update Weapon Durability if Any
        if (_currentWeapon != null)
        {
            if (_currentWeapon.CurrentDurability > 1)
                _currentWeapon.CurrentDurability -= 1;
            else
            {
                _inventory._weapons.Remove(_currentWeapon);
                _currentWeapon = null;
                _attackModifier = 0;
            }
        }


        yield return new WaitForSeconds(0.35f);
        _isAttacking = false;
    }

	public void Die()
	{
		_animator.SetTrigger ("die");
		_isAlive = false;
        _zomzControls.EndZomzMode();
	}

	public GameObject GetClosestObject()
	{
		Collider[] colliders = Physics.OverlapSphere (transform.position, _characterStats.AttackRange);
		Collider closestCollider = null;

		foreach (Collider hit in colliders) 
		{
            ZombieBase zombieControls = hit.gameObject.GetComponent<ZombieBase> ();

			if((hit.GetComponent<Collider>() == transform.GetComponent<Collider>()) || !hit.transform.CompareTag("Enemy"))
			{
				continue;
			}

			if (zombieControls != null && !zombieControls.IsAlive)
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

    IEnumerator Dive()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.forward * _characterStats.DiveDistance;

        _animator.SetTrigger("roll");

        float time = 0;

        while(time < 1)
        {
            transform.position = Vector3.Lerp(startPos, endPos, time);
            time = time / _characterStats.DiveTime + Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        _isDiving = false;
    }

	void Update () 
	{
        if (!_gameData.IsPaused)
        {
            if (_isAlive)
            {
                //Attack
                if (Input.GetKeyDown(KeyCode.Space) && !_zomzControls.ZomzMode.CurrentValue && !_isDiving && !_zomzControls.ZomzMode.CurrentSelectedZombie)
                {
                    if (_canAttack)
                    {
                        BeginAttack();
                    }
                }

                //Debug.Log(_isDiving);

                //DIVE
                if(Input.GetKeyDown(KeyCode.R) && !_zomzControls.ZomzMode.CurrentValue && !_isDiving && !_zomzControls.ZomzMode.CurrentSelectedZombie)
                {
                    _isDiving = true;
                    StartCoroutine(Dive());
                }

                if (!_isAttacking && !_isHurting && !_isDiving)
                {
                    bool running = Input.GetKey(KeyCode.LeftShift);
                    float targetSpeed = ((running) ? _characterStats.RunSpeed : _characterStats.WalkSpeed);
                    _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);

                    Vector3 rightMovement = right * _currentSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
                    Vector3 upMovement = forward * _currentSpeed * Time.deltaTime * Input.GetAxis("Vertical");

                    Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

                    if (_zomzControls.ZomzMode.CurrentValue || _zomzControls.ZomzMode.CurrentSelectedZombie!=null)
                        heading = Vector3.zero;

                    if (heading != Vector3.zero)
                    {
                        transform.forward = heading;
                        transform.position += rightMovement + upMovement;
                    }

                    float animationSpeedPercent = ((running) ? 1 : 0.5f) * heading.magnitude;

                    if (_animator)
                        _animator.SetFloat("speedPercent", animationSpeedPercent, _speedSmoothTime, Time.deltaTime);
                }
            }


            //DEBUG MODE
            if (_debugMode)
            {
                //Die
                if (Input.GetKeyDown(KeyCode.X))
                {
                    Die();
                }

                //Hurt
                if (Input.GetKeyDown(KeyCode.H))
                {
                    //Hurt ();
                }
            }
        }
	}
}
