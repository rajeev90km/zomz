using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnMode{
	SPAWN_EVERY_X_SECONDS = -1,
	LIMIT_MAX_ZOMBIE = 0
}

public class ZombieSpawnerController : MonoBehaviour {
	
	private List<Transform> _allSpawnPoints = new List<Transform>();

	[Header("Spawn Configuration")]
	[SerializeField]
	private SpawnMode _spawnMode;

	[SerializeField]
	[Tooltip("Used with SPAWN_EVERY_X_SECONDS Mode")]
	private float _spawnInterval;

	[SerializeField]
	[Tooltip("Used with LIMIT_MAX_ZOMBIE Mode")]
	private int _maxZombies;

	[Header("Zombie Prefabs")]
	[SerializeField]
	private List<GameObject> _zombieSpawnTypes;

	private Coroutine _spawnEveryXSecondsCoroutine;
	private Coroutine _limitMaxZombiesCoroutine;

	private bool _levelPlaying = false;

	private List<AIStateController> _allZombies = new List<AIStateController> ();

    private bool _zomzModeOn = false;

	void Start () 
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			_allSpawnPoints.Add (transform.GetChild (i));
		}

		LevelStart ();
	}

    public void ToggleZomzMode(bool pEnable)
    {
        _zomzModeOn = pEnable;
    }

	public void LevelStart()
	{
		_levelPlaying = true;

		if (_spawnMode == SpawnMode.SPAWN_EVERY_X_SECONDS)
		{
			_spawnEveryXSecondsCoroutine = StartCoroutine (SpawnEveryXSeconds ());
		}

		if (_spawnMode == SpawnMode.LIMIT_MAX_ZOMBIE)
		{
			SpawnXZombies ();
		}
	}

	void SpawnXZombies()
	{
		for (int i = 0; i < _maxZombies; i++)
		{
			//Spawn zombies from all spawn points sequentially the first time
			SpawnNewZombie ( i % _allSpawnPoints.Count);
		}
	}

	//Spawn Index -1 : Randomly select a spawn point.
	void SpawnNewZombie(int pSpawnIndex = -1)
	{
        if (!_zomzModeOn)
        {
            int newSpawnIndex;

            if (pSpawnIndex == -1)
                newSpawnIndex = Random.Range(0, _allSpawnPoints.Count - 1);
            else
                newSpawnIndex = pSpawnIndex;

            int newZombieIndex = Random.Range(0, _zombieSpawnTypes.Count - 1);
            GameObject newZombie = Instantiate(_zombieSpawnTypes[newZombieIndex], _allSpawnPoints[newSpawnIndex].position, Quaternion.identity) as GameObject;
            _allZombies.Add(newZombie.GetComponent<AIStateController>());
        }
	}

	IEnumerator SpawnEveryXSeconds()
	{
		while (_levelPlaying)
		{
			SpawnNewZombie ();

			yield return new WaitForSeconds(_spawnInterval);
		}
	}


	public void LevelEnd()
	{
		_levelPlaying = false;

		if (_spawnEveryXSecondsCoroutine != null)
		{
			StopCoroutine (_spawnEveryXSecondsCoroutine);
			_spawnEveryXSecondsCoroutine = null;
		}

		if (_limitMaxZombiesCoroutine != null)
		{
			StopCoroutine (_limitMaxZombiesCoroutine);
			_limitMaxZombiesCoroutine = null;
		}
	}

	void Update () 
	{
		for (int i = 0; i < _allZombies.Count; i++)
		{
			if (!_allZombies [i].IsAlive)
			{
				_allZombies.RemoveAt (i);
				SpawnNewZombie ();
			}
		}

	}
}
