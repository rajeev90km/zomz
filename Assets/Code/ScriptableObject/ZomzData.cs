using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Data/New Zomz Data", fileName = "ZomzData_New")]
public class ZomzData : ScriptableObject {

    public bool CurrentValue;

    public ZombieBase CurrentSelectedZombie;

    void OnEnable()
    {
        CurrentValue = false;
    }

}
