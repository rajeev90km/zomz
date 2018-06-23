using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryType {
    HEALTH_PACK = 0,
    WEAPON = 1
}

public class InventoryItem : MonoBehaviour {
    
    [SerializeField]
    private InventoryType _inventoryType;
    public InventoryType InventoryType{
        get { return _inventoryType; }
    }

    [DrawIf("_inventoryType", InventoryType.HEALTH_PACK)]
    [SerializeField]
    private HealthPack _healthPack;
    public HealthPack HealthPack
    {
        get { return _healthPack; }
    }

    [DrawIf("_inventoryType", InventoryType.WEAPON)]
    [SerializeField]
    private Weapon _weapon;
    public Weapon Weapon
    {
        get { return _weapon; }
    }

    [DrawIf("_inventoryType", InventoryType.WEAPON)]
    [SerializeField]
    private int _currentDurability;
    public int CurrentDurability {
        get { return _currentDurability; }
    }

    [Header("Events")]
    [SerializeField]
    private GameEvent _triggerEnterEvent;

    [SerializeField]
    private GameEvent _triggerExitEvent;

    private bool _canEquip = false;

	private void Start()
	{
        if (_inventoryType == InventoryType.WEAPON)
            _currentDurability = _weapon.Durability;
	}

	private void OnTriggerEnter(Collider other)
	{
        if (other.CompareTag("Player"))
        {
            _triggerEnterEvent.Raise();
            _canEquip = true;
        }
	}

	private void OnTriggerExit(Collider other)
	{
        if (other.CompareTag("Player"))
        {
            _triggerExitEvent.Raise();
            _canEquip = false;
        }
	}


    //Check inventory scriptable object
    private bool CanAddToInventory()
    {
        if(_inventoryType==InventoryType.HEALTH_PACK)
            return _healthPack.CanAddToInventory();
        else
            return _weapon.CanAddToInventory();
    }


    //Add to inventory
    private void AddToInventory()
    {
        _triggerExitEvent.Raise();

        if (_inventoryType == InventoryType.HEALTH_PACK)
            _healthPack.Equip(this);
        else
            _weapon.Equip(this);
        
        gameObject.SetActive(false);
    }


	private void Update()
	{
        if(Input.GetKeyDown(KeyCode.X))
        {
            if(_canEquip)
            {
                if (CanAddToInventory())
                {
                    AddToInventory();
                }
                else
                {
                    Debug.Log("Inventory Full");
                }
            }    
        }
	}
}
