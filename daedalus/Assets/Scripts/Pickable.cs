using UnityEngine;
using UnityEngine.Serialization;

public enum PickableType
{
    Treasure,
    WeaponRed,
    WeaponBlue
}


public class Pickable : Entity
{
    [FormerlySerializedAs("_itemType")] public PickableType ItemType;
}