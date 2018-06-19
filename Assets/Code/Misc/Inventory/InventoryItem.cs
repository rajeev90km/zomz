using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : MonoBehaviour {

    [SerializeField]
    private InventoryObject _inventoryObject;

    [Header("Events")]
    [SerializeField]
    private GameEvent _triggerEnterEvent;

    [SerializeField]
    private GameEvent _triggerExitEvent;

    private bool _canEquip = false;

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
        return _inventoryObject.CanAddToInventory();
    }


    //Add to inventory
    private void AddToInventory()
    {
        _triggerExitEvent.Raise();
        _inventoryObject.Equip();
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
