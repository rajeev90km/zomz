using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/AI/State",fileName="State_New")]
public class State : ScriptableObject 
{
	public string AnimationTrigger;
	public Action[] Actions;
	public Color SceneGizmoColor = Color.grey;
	public Transition[] Transitions;


	public void UpdateState(AIStateController pController)
	{
		DoActions (pController);
		CheckTransitions (pController);
	}

	public void DoActions(AIStateController pController)
	{
		for(int i=0;i<Actions.Length;i++)
		{
			Actions [i].Act (pController);
		}
	}

	private void CheckTransitions(AIStateController pController)
	{
		for(int i=0;i<Transitions.Length;i++)
		{
			bool decisionSucceeded = Transitions [i].Decision.Decide (pController);

			if (decisionSucceeded)
			{
				pController.TransitionToState (Transitions [i].TrueState);

			}
			else
			{
				pController.TransitionToState (Transitions [i].FalseState);
			}
		}
	}
	
}
