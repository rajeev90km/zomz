using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCowerAttack : HumanBase 
{
    protected override void Awake(){
        base.Awake();
        _animator.ResetTrigger("walk");
        InitNewState("idle");
    }

    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected override void ExecuteAI()
    {
        Being visibleBeing = GetBeingInLookRange(finalLayerMask, CharacterStats.LookRange);
        float distanceToBeing = Mathf.Infinity;

        if (visibleBeing != null)
            distanceToBeing = Vector3.Distance(transform.position, visibleBeing.transform.position);

        ////Transition to ATTACK if in attack range
        if ((distanceToBeing <= CharacterStats.AttackRange))
        {
            _animator.ResetTrigger("walk");
            _currentState = HumanStates.ATTACK;
            InitNewState("attack", true);
            _previousState = _currentState;
        }
        else
        {
            _currentState = HumanStates.NONE;
            InitNewState("idle");
            _previousState = _currentState;
        }


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
