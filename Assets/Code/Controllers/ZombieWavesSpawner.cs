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

[System.Serializable]
public class ZombieWave
{
    public string waveNum;

    public List<ZombieSpawn> ZombieSpawns;

    public bool PlayInterstitalAfterWave;

    public Conversation InterstitalToPlay;
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

    private bool _levelEnded = false;

	private void Start()
    {
        _deadZombies.Add(null);
    }

	public void StartNextWave()
    {
        if (!_levelEnded)
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

	private void Update()
	{
        if (!_levelEnded)
        {
            for (int i = 0; i < _allZombies.Count; i++)
            {
                if (!_allZombies[i].IsAlive)
                {
                    if (!_deadZombies.Contains(_allZombies[i]))
                        _deadZombies.Add(_allZombies[i]);
                }
            }

            if (_deadZombies.Count == _allZombies.Count)
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
	}

}
