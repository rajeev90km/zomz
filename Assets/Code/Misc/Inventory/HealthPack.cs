using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zomz/Inventory Item/New HealthPack", fileName = "HealthPack_New")]
public class HealthPack : InventoryObject
{
    public float Health;

    public override void Use()
    {
        Debug.Log("Health Pack Use");
    }

    public override void Equip()
    {
        Inventory._healthPacks.Add(this);
    }

    public override bool CanAddToInventory()
    {
        if (Inventory._healthPacks.Count < Inventory.MAX_HEALTH_PACKS)
            return true;

        return false;
    }
}
