using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Inventory Item/New Weapon", fileName = "Weapon_New")]
public class Weapon : InventoryObject 
{
    public GameObject Model;

    public int AttackStrength;

    public int Durability;

    public override void Use(InventoryItem pItem, CharacterControls pControls)
    {
        pControls.CurrentWeapon = pItem;
        pControls.AttackModifier = AttackStrength;
    }

    public override void Equip(InventoryItem pItem)
    {
        Inventory._weapons.Add(pItem);
    }

	public override bool CanAddToInventory()
	{
        if (Inventory._weapons.Count < Inventory.MAX_WEAPONS)
            return true;

        return false;
	}
}
