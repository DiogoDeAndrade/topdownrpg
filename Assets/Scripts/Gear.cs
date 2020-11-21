using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gear", menuName = "RPG/Items/Gear")]
public class Gear : Item
{
    public enum Slot { Weapon, Armour };

    public Slot slot;
    public int  armourSetId = -1;
}
