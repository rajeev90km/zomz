using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetShaderProperty : MonoBehaviour 
{
	[SerializeField]
	private Material _material;

	[SerializeField]
	private string _propertyName;

	[SerializeField]
	private Transform _transform;

	void Awake()
	{
		if(_transform == null)
			_transform = this.transform;
	}

	void Update () 
	{
		_material.SetVector (_propertyName, transform.position);
	}
}
