using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum ZomzAction
{
	NONE = 0,
	MOVE = 1,
	ATTACK = 2
}

[System.Serializable]
public class ZomzActionPoint
{
	public AIStateController Zomz;
	public Vector3 Position;
	public ZomzAction ZomzAction;
	public Transform ActionTarget;

	public ZomzActionPoint(Vector3 pPosition, ZomzAction pAction, Transform pTarget)
	{
		Position = pPosition;
		ZomzAction = pAction;
		ActionTarget = pTarget;
	}

	public ZomzActionPoint(AIStateController pZomz, Vector3 pPosition, ZomzAction pAction, Transform pTarget)
	{
		Zomz = pZomz;
		Position = pPosition;
		ZomzAction = pAction;
		ActionTarget = pTarget;
	}

}

[DisallowMultipleComponent]
public class ZomzControls : MonoBehaviour {

	private bool _zomzMode = false;
	public bool ZomzMode
	{
		get{ return _zomzMode;}	
	}

	private int _enemyLayerMask;
	private CharacterControls _characterControls;
	private Animator _animator;

	private List<AIStateController> _zombiesUnderControl;
	public int NumZombiesUnderControl
	{
		get { return _zombiesUnderControl.Count; }
	}

	private bool _canUseZomzMode = true;

	private const float ZOMZ_COOLDOWN_TIME = 5f;

    [SerializeField]
    private GameData _gameData;

	[SerializeField]
	private GameFloatAttribute _zomzManaAttribute;

	[SerializeField]
	private ZomzListAttribute _zomzActionsList;

	[Header("Debug")]
	[SerializeField]
	private GameObject _debugCanvas;

	[Header("Events")]
	[SerializeField]
	private GameEvent _zomzStartEvent;

	[SerializeField]
	private GameEvent _zomzEndEvent;


	void Start () 
	{
		_animator = GetComponent<Animator> ();
		_zombiesUnderControl = new List<AIStateController> ();
		_characterControls = GetComponent<CharacterControls> ();
		_enemyLayerMask = (1 << LayerMask.NameToLayer ("Enemy"));
	}

	void OnDrawGizmos()
	{
		if (_characterControls != null)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere (transform.position, _characterControls.CharacterStats.ZomzRange);
		}
	}

	void Update () 
	{
        if (!_gameData.IsPaused)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (_zomzMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, _enemyLayerMask))
                    {
                        if (hit.transform != null)
                        {
                            for (int i = 0; i < _zombiesUnderControl.Count; i++)
                            {
                                if (hit.collider.gameObject != _zombiesUnderControl[i].gameObject)
                                    _zombiesUnderControl[i].ClearCurrentControl();
                                else
                                    _zombiesUnderControl[i].SelectCurrentForControl();
                            }

                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                //TODO check for null and override
                StartCoroutine(ToggleZomzMode());
            }
        }
	}

	IEnumerator ToggleZomzMode()
	{

		if (_canUseZomzMode)
		{
			_zomzMode = !_zomzMode;

			//_debugCanvas.SetActive (_zomzMode);

			if (_zomzMode)
			{
				if (_zomzManaAttribute)
					_zomzManaAttribute.ResetAttribute ();

				if (_zomzStartEvent)
					_zomzStartEvent.Raise ();


                _zomzActionsList.ResetList();

				_zombiesUnderControl.Clear ();

				_animator.SetFloat ("speedPercent",0.0f);

				Collider[] _zombiesHit = Physics.OverlapSphere (transform.position, _characterControls.CharacterStats.ZomzRange, _enemyLayerMask);

				for (int i = 0; i < _zombiesHit.Length; i++)
				{
					AIStateController zCtrl = _zombiesHit [i].GetComponent<AIStateController> ();

					if (zCtrl != null && zCtrl.IsAlive)
					{
						zCtrl.TakeControl ();
						_zombiesUnderControl.Add (zCtrl);
					}
				}
			} 
			else
			{
                Dictionary<AIStateController, int> _zomzActionCountPerZombie = new Dictionary<AIStateController, int>();

                //count actions for each zombie
                for (int i = 0; i < _zomzActionsList.AllActionPoints.Count; i++)
                {
                    if (!_zomzActionCountPerZombie.ContainsKey(_zomzActionsList.AllActionPoints[i].Zomz))
                        _zomzActionCountPerZombie.Add(_zomzActionsList.AllActionPoints[i].Zomz, 1);
                    else
                        _zomzActionCountPerZombie[_zomzActionsList.AllActionPoints[i].Zomz]++;
                }


                for (int i = 0; i < _zombiesUnderControl.Count; i++)
                {
                    if(_zomzActionCountPerZombie.ContainsKey(_zombiesUnderControl[i]))
                    {
                        if(_zomzActionCountPerZombie[_zombiesUnderControl[i]]>0) 
                            _zombiesUnderControl[i].Animator.SetTrigger("walk");
                    }
                }


                for (int i = 0; i < _zombiesUnderControl.Count; i++)
                {
                    _zombiesUnderControl[i].BeforeExecuting();
                    _zombiesUnderControl[i].navMeshAgent.ResetPath();
                }

				_canUseZomzMode = false;

                AIStateController currentZomz = null;
                ZomzAction currentAction = ZomzAction.NONE;

                if (_zomzActionsList.AllActionPoints.Count > 0)
                    currentZomz = _zomzActionsList.AllActionPoints[0].Zomz;

                if (_zomzEndEvent != null)
                    _zomzEndEvent.Raise();

                for (int i = 0; i < _zomzActionsList.AllActionPoints.Count; i++)
                {
                    //MOVE
                    if (_zomzActionsList.AllActionPoints[i].ZomzAction == ZomzAction.MOVE)
                    {
                        if(currentZomz!=_zomzActionsList.AllActionPoints[i].Zomz)
                        {
                            currentZomz.Animator.SetTrigger("idle");
                            _zomzActionsList.AllActionPoints[i].Zomz.Animator.SetTrigger("walk");
                            currentZomz = _zomzActionsList.AllActionPoints[i].Zomz;
                        }

                        if(currentZomz==_zomzActionsList.AllActionPoints[i].Zomz && currentAction==ZomzAction.ATTACK)
                        {
                            currentZomz.Animator.SetTrigger("walk");
                        }

                        currentAction = ZomzAction.MOVE;
                        _zomzActionsList.AllActionPoints[i].Zomz.navMeshAgent.SetDestination(_zomzActionsList.AllActionPoints[i].Position);
                        yield return new WaitUntil(() => _zomzActionsList.AllActionPoints[i].Zomz.navMeshAgent.hasPath == false);
                    }
                    //ATTACK
                    else if (_zomzActionsList.AllActionPoints[i].ZomzAction == ZomzAction.ATTACK)
                    {
                        currentAction = ZomzAction.ATTACK;
                        _zomzActionsList.AllActionPoints[i].Zomz.Animator.SetTrigger("attack");
                        yield return new WaitForSeconds(1f);
                        _zomzActionsList.AllActionPoints[i].Zomz.DealZomzDamage(_zomzActionsList.AllActionPoints[i].ActionTarget);
                        yield return new WaitForSeconds(1f);
                    }
                }                    


				for (int i = 0; i < _zombiesUnderControl.Count; i++)
				{
					_zombiesUnderControl [i].RelinquishControl ();
				}

				_zombiesUnderControl.Clear ();


				StartCoroutine (ZomzCoolDown ());

                yield return null;
			}
		}
	}

	IEnumerator ZomzCoolDown()
	{
		float time = 0f;

		float curVal = _zomzManaAttribute.CurrentValue;
		float coolDownTime = ZOMZ_COOLDOWN_TIME - ((curVal / 100) * ZOMZ_COOLDOWN_TIME);

		while (time < 1)
		{
			_zomzManaAttribute.CurrentValue = Mathf.Lerp (curVal, 100, time);
			time += Time.deltaTime / coolDownTime;
			yield return null;
		}

		_zomzManaAttribute.CurrentValue = 100;
		_canUseZomzMode = true;

		yield return null;
	}
}
