using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="OM_New", menuName="Zomz/New Object Marker")]
public class ObjectMarker : ScriptableObject {

	[SerializeField]
	private Transform _marker;
	public Transform Marker
	{
		get { return _marker; }
		set{ _marker = value; }
	}
}
