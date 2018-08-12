using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Data/New Zomz Data", fileName = "ZomzData_New")]
public class ZomzData : ScriptableObject {

    public bool CurrentValue;

    public bool IsRegistered = false;

    public ZombieBase CurrentSelectedZombie;

    public ZomzManaConsumeType ManaConsumeType;

    void OnEnable()
    {
        CurrentValue = false;
        CurrentSelectedZombie = null;
        IsRegistered = false;
    }

}
