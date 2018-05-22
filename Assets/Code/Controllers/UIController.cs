using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    [SerializeField]
    private SelectedZombie _currentSelectedZombie;

	[Header("Player Health UI")]
	[SerializeField]
	private Image _playerHealthBar;

	[Header("Zomz UI")]
	[SerializeField]
	private GameFloatAttribute _zomzManaAttribute;

	[SerializeField]
	private GameObject _zomzText;

	[SerializeField]
	private Image _zomzManaBar;

    [SerializeField]
    private GameObject _attackUI;

	private CharacterControls _playerStats;
	private GameObject _playerObj;

	private void Start()
	{
		_playerObj = GameObject.FindWithTag ("Player");

		if (_playerObj != null)
			_playerStats = _playerObj.GetComponent<CharacterControls> ();
			
	}

	public void DisplayZomzUI(bool pEnable)
	{
		if (_zomzText)
			_zomzText.gameObject.SetActive (pEnable);
	}

	void Update()
	{
        if (_currentSelectedZombie.CurrentSelectedZombie != null)
            _attackUI.SetActive(true);
        else
            _attackUI.SetActive(false);

		if (_zomzManaBar && _zomzManaAttribute)
			_zomzManaBar.fillAmount = _zomzManaAttribute.CurrentValue/100;

		if (_playerHealthBar && _playerStats)
			_playerHealthBar.fillAmount = _playerStats._currentHealth / 100;
	}


}
