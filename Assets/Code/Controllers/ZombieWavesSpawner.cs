using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZombieSpawn
{
    public Transform SpawnPoint;
    public GameObject ZombieToSpawn;

    public ZombieSpawn(Transform pSpawnPoint, GameObject pZombieToSpawn)
    {
        SpawnPoint = pSpawnPoint;
        ZombieToSpawn = pZombieToSpawn;
    }
}

public enum WaveType
{
    TIME_BASED = 0,
    PROXIMITY_BASED = 1,
    KILL_ALL_ZOMBIES = 2
}

[System.Serializable]
public class ZombieWave
{
    public string waveNum;

    public List<ZombieSpawn> ZombieSpawns;

    public bool PlayInterstitalAfterWave;

    public Conversation InterstitalToPlay;

    public WaveType WaveType;

    [DrawIf("WaveType", WaveType.TIME_BASED)]
    public float timeTillNextWave;
}

public class ZombieWavesSpawner : ZombieBaseSpawner 
{
    private int currentWave = -1;
    private List<AIStateController> _deadZombies = new List<AIStateController>();

    [SerializeField]
    private List<ZombieWave> _zombieWaveInfo = new List<ZombieWave>();

    [Header("Events")]
    [SerializeField]
    private GameEvent _conversationStartEvent;

    [SerializeField]
    private GameEvent _levelEndEvent;

    [SerializeField]
    private GameEvent _levelLostEvent;

    private bool _levelEnded = false;
    private bool _levelLost = false;

    private GameObject _playerObj;
    private CharacterControls _playerControls;

	private void Start()
    {
        _deadZombies.Add(null);
        _playerObj = GameObject.FindWithTag("Player");
        if (_playerObj)
            _playerControls = _playerObj.GetComponent<CharacterControls>();
    }

	public void StartNextWave()
    {
        if (!_levelEnded && !_levelLost)
        {
            //Increment Current Wave
            currentWave += 1;

            _deadZombies.Clear();
            _allZombies.Clear();

            //Process next wave
            if (currentWave <= _zombieWaveInfo.Count - 1)
            {
                //Spawn all zombies per wave
                for (int i = 0; i < _zombieWaveInfo[currentWave].ZombieSpawns.Count; i++)
                {
                    GameObject _zombie = Instantiate(_zombieWaveInfo[currentWave].ZombieSpawns[i].ZombieToSpawn, _zombieWaveInfo[currentWave].ZombieSpawns[i].SpawnPoint.position, Quaternion.identity) as GameObject;
                    AIStateController asc = _zombie.GetComponent<AIStateController>();
                    if (asc != null)
                        _allZombies.Add(asc);
                }

                if(_zombieWaveInfo[currentWave].WaveType == WaveType.TIME_BASED)
                {
                    StartCoroutine(WaitAndProcessNextWave());
                }
            }
            //All Waves complete for this level
            else
            {
                _allZombies.Clear();
                _deadZombies.Clear();
                _levelEndEvent.Raise();
                _levelEnded = true;
            }
        }
    }

    public void ProcessTriggeredWave()
    {
        if (_zombieWaveInfo[currentWave].WaveType == WaveType.PROXIMITY_BASED)
        {
            Debug.Log("New Wave Triggered");
        }
    }

    public IEnumerator WaitAndProcessNextWave()
    {
        yield return new WaitForSeconds(_zombieWaveInfo[currentWave].timeTillNextWave);

        if (_zombieWaveInfo[currentWave].PlayInterstitalAfterWave)
        {
            if (_zombieWaveInfo[currentWave].InterstitalToPlay != null)
            {
                _gameData.CurrentConversation.Conversation = _zombieWaveInfo[currentWave].InterstitalToPlay;
                _conversationStartEvent.Raise();
            }
        }
        else
            StartNextWave();

        _deadZombies.Clear();
        _deadZombies.Add(null);

    }

	private void Update()
	{
        if (!_levelEnded && !_levelLost)
        {
            for (int i = 0; i < _allZombies.Count; i++)
            {
                if (!_allZombies[i].IsAlive)
                {
                    if (!_deadZombies.Contains(_allZombies[i]))
                        _deadZombies.Add(_allZombies[i]);
                }
            }


            if (currentWave > -1 && _zombieWaveInfo[currentWave].WaveType == WaveType.KILL_ALL_ZOMBIES && _deadZombies.Count == _allZombies.Count)
            {
                if (currentWave <= _zombieWaveInfo.Count - 1)
                {
                    //Change logic
                    if (_zombieWaveInfo[currentWave].PlayInterstitalAfterWave)
                    {
                        if (_zombieWaveInfo[currentWave].InterstitalToPlay != null)
                        {
                            _gameData.CurrentConversation.Conversation = _zombieWaveInfo[currentWave].InterstitalToPlay;
                            _conversationStartEvent.Raise();
                        }
                    }
                    else
                        StartNextWave();

                    _deadZombies.Clear();
                    _deadZombies.Add(null);
                }
            }
        }

        if(!_playerControls.IsAlive && !_levelLost)
        {
            _levelLost = true;
            _allZombies.Clear();
            _deadZombies.Clear();
            _levelLostEvent.Raise();
            _levelEnded = true;
        }

	}

}
