using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZomzController : MonoBehaviour {

    public ZomzData ZomzMode;

    private int _enemyLayerMask;
    private CharacterControls _characterControls;
    private Animator _animator;

    private const float ZOMZ_COOLDOWN_TIME = 5f;

    private bool _canUseZomzMode = true;
    public bool CanUseZomzMode
    {
        get { return _canUseZomzMode; }
        set { _canUseZomzMode = value; }
    }

    [SerializeField]
    private GameData _gameData;

    [SerializeField]
    private GameFloatAttribute _zomzManaAttribute;

    [Header("Miscellaneous")]
    [SerializeField]
    private GameObject _arrowPrefab;

    [Header("Events")]
    [SerializeField]
    private GameEvent _zomzStartEvent;

    [SerializeField]
    private GameEvent _zomzEndEvent;

    [SerializeField]
    private GameEvent _zomzRegisterEvent;

    [SerializeField]
    private GameEvent _zomzUnregisterEvent;

    private List<ZombieBase> _zombiesUnderControl;
    private GameObject _pointerArrowObj;

	void Awake () 
    {
        //Cache Properties
        _animator = GetComponent<Animator>();
        _characterControls = GetComponent<CharacterControls>();

        _pointerArrowObj = Instantiate(_arrowPrefab) as GameObject;
        _pointerArrowObj.SetActive(false);

        _zombiesUnderControl = new List<ZombieBase>();
        _enemyLayerMask = (1 << LayerMask.NameToLayer("Enemy"));
	}

    public void ProcessZomzMode()
    {
        if(!ZomzMode.CurrentValue)
        {
            _zombiesUnderControl.Clear();
            ZomzMode.CurrentValue = true;

            if (ZomzMode.CurrentValue)
            {
                _zomzStartEvent.Raise();
                _zomzManaAttribute.ResetAttribute();

                _animator.SetFloat("speedPercent", 0.0f);

                Collider[] _zombiesHit = Physics.OverlapSphere(transform.position, _characterControls.CharacterStats.ZomzRange, _enemyLayerMask);

                for (int i = 0; i < _zombiesHit.Length; i++)
                {
                    ZombieBase zombie = _zombiesHit[i].GetComponent<ZombieBase>();
                    _zombiesUnderControl.Add(zombie);

                    if (zombie != null && zombie.IsAlive)
                    {
                        zombie.OnZomzModeAffected();
                    }
                }
            }
        }
    }

    public void RegisterZomzMode()
    {
        if (ZomzMode.CurrentSelectedZombie)
        {
            ZomzMode.CurrentSelectedZombie.OnZomzModeRegister();
            _zomzRegisterEvent.Raise();
        }
        _zomzEndEvent.Raise();
        ZomzMode.CurrentValue = false;
        _pointerArrowObj.SetActive(false);
    }

    public void EndZomzMode()
    {
        _pointerArrowObj.SetActive(false);
        ZomzMode.CurrentSelectedZombie = null;
        ZomzMode.CurrentValue = false;
        _zomzEndEvent.Raise();    
    }
	
    void OnDrawGizmos()
    {
        if (_characterControls != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _characterControls.CharacterStats.ZomzRange);
        }
    }

	void Update () 
    {
        if (!_gameData.IsPaused)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (ZomzMode.CurrentValue)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, _enemyLayerMask))
                    {
                        if (hit.transform != null)
                        {
                            _pointerArrowObj.SetActive(true);
                            ZomzMode.CurrentSelectedZombie = hit.transform.gameObject.GetComponent<ZombieBase>();

                            Vector3 zombiePos = hit.transform.gameObject.transform.position;
                            _pointerArrowObj.transform.position = new Vector3(zombiePos.x, 3, zombiePos.z);

                        }
                    }
                }
            }

            //Request Zomz Mode
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (!ZomzMode.CurrentValue)
                    ProcessZomzMode();
                else
                    RegisterZomzMode();
            }

            //End Zomz Mode
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndZomzMode();
            }
        }
	}
}
