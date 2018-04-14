using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="StoryProgressMonitor", menuName="Zomz/New Story Progress Monitor")]
public class StoryProgressMonitor : ScriptableObject
{
	[HideInInspector]
	public bool isNodePlaying = false;
}
