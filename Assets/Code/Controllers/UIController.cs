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

    [Header("Inventory UI")]
    [SerializeField]
    private GameObject _inventoryCanvas;

    [SerializeField]
    private Inventory _inventory;

    [SerializeField]
    private Text _inventoryMessages;

    [SerializeField]
    private GameObject _healthPackRow;

    [SerializeField]
    private GameObject _weaponRow;

    [Header("Events")]
    [SerializeField]
    private GameEvent _inventoryTriggerEnter;

    [SerializeField]
    private GameEvent _inventoryTriggerExit;

	private CharacterControls _playerStats;
	private GameObject _playerObj;

    private const string INVENTORY_EQUIP_MESSAGE = "Press 'X' to Equip Item.";
    private const string INVENTORY_FULL_WARNING = "No Inventory slots available. Use some to free up slots.";
    private const int INVENTORY_UI_ROW_OFFSET = 80;

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

    public void ToggleInventoryMessage(bool pEnable)
    {
        if(pEnable)
        {
            _inventoryMessages.text = INVENTORY_EQUIP_MESSAGE;    
        }
        else
        {
            _inventoryMessages.text = "";    
        }
    }

    public void OpenInventory()
    {
        _inventoryCanvas.SetActive(true);

        for (int i = 0; i < _inventory._healthPacks.Count; i++)
        {
            GameObject hp = Instantiate(_healthPackRow) as GameObject;
            hp.transform.parent = _inventoryCanvas.transform;
            hp.GetComponent<RectTransform>().localPosition = new Vector3(0, -i * INVENTORY_UI_ROW_OFFSET, 0);
            hp.GetComponent<RectTransform>().localScale = Vector3.one;

            Text hpText = hp.transform.Find("HealthText").gameObject.GetComponent<Text>();
            hpText.text = "Health:   " + _inventory._healthPacks[i].Health;

            Button hpButton = hp.transform.Find("Button").gameObject.GetComponent<Button>();
            HealthPack h = _inventory._healthPacks[i];
            hpButton.onClick.AddListener(delegate { UseHealthPack(h,hp); });
        }
    }

    public void UseHealthPack(HealthPack pHealthPack, GameObject pRow)
    {
        if (_playerStats._currentHealth < 100)
        {
            pHealthPack.Use(_playerStats);
            DestroyImmediate(pRow);
        }
    }

    public void CloseInventory()
    {
        _inventoryCanvas.SetActive(false);
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
