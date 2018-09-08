using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFlee : HumanBase 
{
    bool _isFleePointFound = false;

    // MAIN AI LOOP - GOES THROUGH LIST OF ACTIONS AND DECIDES STATE OF AI
    protected override void ExecuteAI()
    {
        Being visibleBeing = GetBeingInLookRange(finalLayerMask, CharacterStats.LookRange);
        float distanceToBeing = Mathf.Infinity;
        Vector3 beingDirection = Vector3.zero;
        float beingAngle = Mathf.Infinity;
        bool unobstructedViewToBeing = false;

        if (visibleBeing != null)
        {
            distanceToBeing = Vector3.Distance(transform.position, visibleBeing.transform.position);
            beingDirection = visibleBeing.transform.position - transform.position;
            beingAngle = Vector3.Angle(beingDirection, transform.forward);

            //if (visibleBeing.CompareTag("Player"))
            //{
            //    RaycastHit hit;

            //    Debug.DrawRay(transform.position + transform.forward + transform.up * _sightHeightMultiplier, beingDirection, Color.green);

            //    ownCollider.enabled = false;
            //    if (Physics.Raycast(transform.position + transform.up * _sightHeightMultiplier, beingDirection, out hit, Mathf.Infinity))
            //    {
            //        if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Human"))
            //        {
            //            unobstructedViewToBeing = true;
            //        }
            //    }
            //    ownCollider.enabled = true;
            //}
            //else
            unobstructedViewToBeing = true;
        }

        if (visibleBeing == null)
        {
            _isFleePointFound = false;
            _currentState = HumanStates.WALK;
            InitNewState("walk");
            _previousState = _currentState;
        }
        //Transition to FLEE mode if close enough to the a being
        else if (visibleBeing != null && visibleBeing.IsAlive && unobstructedViewToBeing )// && beingAngle < CharacterStats.FieldOfView * 0.5f)
        {
            _animator.ResetTrigger("walk");
            _currentState = HumanStates.FLEE;
            InitNewState("run", false);
            _previousState = _currentState;
        }
        //Transition to ATTACK if in attack range
        //else if ((distanceToBeing <= CharacterStats.AttackRange))
        //{
        //    _isFleePointFound = false;
        //    _animator.ResetTrigger("walk");
        //    _currentState = HumanStates.ATTACK;
        //    InitNewState("attack", true);
        //    _previousState = _currentState;
        //}
        else
        {
            _isFleePointFound = false;
            _currentState = HumanStates.WALK;
            InitNewState("walk");
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



    protected override void FleeState()
    {
        if (_isAlive)
        {
            if (_navMeshAgent.isActiveAndEnabled)
            {
                if(!_isFleePointFound)
                {
                    GetExclusiveNextWayPoint();
                    _isFleePointFound = true;
                }

                _navMeshAgent.speed = CharacterStats.RunSpeed;
                _navMeshAgent.destination = _wayPoints[_nextWayPoint].position;
                _navMeshAgent.isStopped = false;

                if (Vector3.Distance(transform.position, _wayPoints[_nextWayPoint].position) <= 1f)
                {
                    _isFleePointFound = false;
                    GetNextWayPoint();
                }
            }

        }
    }


}
