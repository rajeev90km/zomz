using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetObjectMarker : MonoBehaviour {

	[SerializeField]
	private ObjectMarker _objectMarker;

	IEnumerator Start () {

		yield return new WaitForEndOfFrame ();

		if (_objectMarker != null)
		{
			_objectMarker.Marker = transform;
		}

	}
}
