using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Inventory Item/New Weapon", fileName = "Weapon_New")]
public class Weapon : InventoryObject 
{
    public GameObject Model;

    public float AttackStrength;

    public float Durability;

    public override void Use()
    {
        Debug.Log("Weapon Use");
    }

    public override void Equip()
    {
        Inventory._weapons.Add(this);
    }

	public override bool CanAddToInventory()
	{
        if (Inventory._weapons.Count < Inventory.MAX_WEAPONS)
            return true;

        return false;
	}
}
