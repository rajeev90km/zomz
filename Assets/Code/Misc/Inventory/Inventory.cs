using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/New Inventory", fileName = "Inventory_New")]
public class Inventory : ScriptableObject 
{
    public int MAX_HEALTH_PACKS = 3;

    public int MAX_WEAPONS = 3;

    public List<InventoryItem> _healthPacks;

    public List<InventoryItem> _weapons;

	private void OnEnable()
	{
        _healthPacks.Clear();
        _weapons.Clear();
	}
}
