using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

	[Header("Zomz UI")]
	[SerializeField]
	private GameFloatAttribute _zomzManaAttribute;

	[SerializeField]
	private GameObject _zomzText;

	[SerializeField]
	private Image _zomzManaBar;

	public void DisplayZomzCanvas(bool pEnable)
	{
		if (_zomzText)
			_zomzText.gameObject.SetActive (pEnable);
	}


	void Update()
	{
		if (_zomzManaBar && _zomzManaAttribute)
			_zomzManaBar.fillAmount = _zomzManaAttribute.CurrentValue/100;
	}


}
