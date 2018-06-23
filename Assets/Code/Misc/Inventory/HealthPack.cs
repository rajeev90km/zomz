using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Inventory Item/New HealthPack", fileName = "HealthPack_New")]
public class HealthPack : InventoryObject
{
    public float Health;

    public override void Use(InventoryItem pItem,CharacterControls pControls)
    {
        pControls._currentHealth += Health;

        if (pControls._currentHealth > 100)
            pControls._currentHealth = 100;
    }

    public override void Equip(InventoryItem pItem)
    {
        Inventory._healthPacks.Add(pItem);
    }

    public override bool CanAddToInventory()
    {
        if (Inventory._healthPacks.Count < Inventory.MAX_HEALTH_PACKS)
            return true;

        return false;
    }
}
