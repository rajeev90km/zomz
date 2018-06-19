using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryObject : ScriptableObject 
{
    public Inventory Inventory;

    public abstract void Use();

    public abstract void Equip();

    public abstract bool CanAddToInventory();
}