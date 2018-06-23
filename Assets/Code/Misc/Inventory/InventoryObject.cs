using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryObject : ScriptableObject 
{
    public Inventory Inventory;

    public abstract void Use(InventoryItem pItem, CharacterControls pControls);

    public abstract void Equip(InventoryItem pItem);

    public abstract bool CanAddToInventory();
}