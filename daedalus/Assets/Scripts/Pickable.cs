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
    public PickableType ItemType;
    public Sprite UIIcon;
}