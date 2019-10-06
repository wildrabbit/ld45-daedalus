using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New TileSet", menuName = "DAEDALUS/TileSet")]
public class TileSet : ScriptableObject
{
    public TileData[] tiles;

    public TileData GetByType(TileType type)
    {
        return System.Array.Find(tiles, x => x.TileType == type);
    }

    public TileData GetByTile(TileBase leTile)
    {
        return System.Array.Find(tiles, x => x.UnityTile == leTile);
    }
}
