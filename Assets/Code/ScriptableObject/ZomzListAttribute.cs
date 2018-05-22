using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Zomz/GameAttribute/New Zombie List Attribute",fileName="GA_ZomzList_New")]
public class ZomzListAttribute : GameAttribute {

    public List<ZomzActionPoint> AllActionPoints;

	private void OnEnable()
	{
        AllActionPoints = new List<ZomzActionPoint>();
	}

    public void ResetList()
    {
        AllActionPoints.Clear();
    }

	private void OnDisable()
	{
        ResetList();
	}

}
