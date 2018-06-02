using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZombieBaseSpawner : MonoBehaviour {

    [SerializeField]
    protected GameData _gameData;

    protected List<AIStateController> _allZombies = new List<AIStateController> ();

    protected bool _zomzModeOn = false;
}
