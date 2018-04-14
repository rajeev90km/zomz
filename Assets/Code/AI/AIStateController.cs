﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateController : MonoBehaviour {

	[SerializeField]
	private Transform _eyes;
	public Transform Eyes
	{
		get { return _eyes; }
		set { _eyes = value; }
	}

	[SerializeField]
	private float _lookRange = 10f;
	public float LookRange
	{
		get{ return _lookRange; }
		set{ _lookRange = value; }
	}

	[SerializeField]
	private float _lookSphere = 10f;
	public float LookSphere
	{
		get{ return _lookSphere; }
		set{ _lookSphere = value; }
	}

	[SerializeField]
	private float _attackRange = 1f;
	public float AttackRange
	{
		get{ return _attackRange; }
		set{ _attackRange = value; }
	}

	[SerializeField]
	private float _attackRate = 1f;
	public float AttackRate
	{
		get{ return _attackRate; }
		set{ _attackRate = value; }
	}

	[SerializeField]
	private State _currentState;
	public State CurrentState
	{
		get{ return _currentState; }
		set{ _currentState = value; }
	}


	[SerializeField]
	private State _remainState;
	public State RemainState
	{
		get{ return _remainState; }
		set{ _remainState = value; }
	}

	private Animator _animator;
	public Animator Animator
	{
		get{ return _animator;}
	}

	[HideInInspector]
	public NavMeshAgent navMeshAgent;

	[HideInInspector]
	public Transform ChaseTarget;

	[HideInInspector]
	public float StateTimeElapsed = 0f;

	[SerializeField]
	private GameObject _wayPointsObj;

	[HideInInspector]
	public List<Transform> wayPoints;

	private int _nextWayPoint;
	public int NextWayPoint
	{
		get{ return _nextWayPoint; }
		set{ _nextWayPoint = value; }
	}

	void Start () 
	{
		//Get all waypoints
		if (_wayPointsObj != null)
		{
			for(int i=0;i<_wayPointsObj.transform.GetChildCount();i++)
			{
				wayPoints.Add(_wayPointsObj.transform.GetChild(i));
			}
		}

		//Get Navmesh Agent
		navMeshAgent = GetComponent<NavMeshAgent>();

		//Set Animator
		_animator = GetComponent<Animator>();
		Debug.Log (_currentState.AnimationTrigger);
		_animator.SetTrigger (_currentState.AnimationTrigger);
	}

	void Update () 
	{ 
		CurrentState.UpdateState (this);
	}

	void OnDrawGizmos()
	{
		if (_currentState != null)
		{
			Gizmos.color = _currentState.SceneGizmoColor;
			Gizmos.DrawWireSphere (transform.position, 2);
		}
	}

	public void TransitionToState(State pNextState)
	{
		if (pNextState != RemainState)
		{
			navMeshAgent.Stop ();
			_currentState = pNextState;
			_animator.SetTrigger (_currentState.AnimationTrigger);
			OnExitState ();
		}
	}

	public bool CheckIfCountDownElapsed(float duration)
	{
		StateTimeElapsed += Time.deltaTime;
		return (StateTimeElapsed >= duration);
	}

	private void OnExitState()
	{
		StateTimeElapsed = 0;
	}
}
