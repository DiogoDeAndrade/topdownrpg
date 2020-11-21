using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "RPG/Items/Generic")]
public class Item : ScriptableObject
{
    public enum Type { Wearable, Potion };

    public Type type;
}
