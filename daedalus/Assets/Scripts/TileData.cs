using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public enum TileType
{
    None = -1,
    Wall,
    Ground
}

[CreateAssetMenu(fileName = "New TileData", menuName = "DAEDALUS/TileData")]
public class TileData : ScriptableObject
{
    public string TileID => UnityTile.name;
    public TileType TileType;
    public TileBase UnityTile;
    public bool Walkable;
    public bool BlocksPlacement;
}
