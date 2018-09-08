using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCower : HumanBase 
{
    protected override void Awake(){
        base.Awake();
        _animator.ResetTrigger("walk");
        InitNewState("idle");
    }

    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected override void ExecuteAI()
    {
        
        _currentState = HumanStates.NONE;
        InitNewState("idle");
        _previousState = _currentState;


        switch (_currentState)
        {
            case HumanStates.WALK:
                WalkState();
                break;
            case HumanStates.CHASE:
                ChaseState();
                break;
            case HumanStates.ATTACK:
                AttackState();
                break;
            case HumanStates.FLEE:
                FleeState();
                break;
            default:
                break;
        }
    }

}
