using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/GameAttribute/New Selected Zombie Attribute", fileName = "GA_CurrentSelectedZombie")]
public class SelectedZombie : ScriptableObject 
{
    public AIStateController CurrentSelectedZombie;
	
    public void ResetSelection()
    {
        CurrentSelectedZombie = null;
    }

    public void EnableAttack()
    {
        if(CurrentSelectedZombie!=null)
        {
            CurrentSelectedZombie.ToggleZomzAttackMode(true);
        }
    }

	private void OnEnable()
	{
        ResetSelection();
	}

	private void OnDisable()
	{
        ResetSelection();
	}
}
