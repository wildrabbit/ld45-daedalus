using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceableBlock : MonoBehaviour
{
    [SerializeField] Tilemap _map;
    public BoundsInt Bounds => _map.cellBounds;
    public TileBase[] Tiles =>_map.GetTilesBlock(_map.cellBounds);

    public void SetTint(Color color)
    {
        _map.color = color;
    }

    public TileBase TileAt(int x, int y)
    {
        Vector3Int coords = new Vector3Int(x, y, 0);
        return _map.GetTile(coords);
    }

    public void RefreshBounds()
    {
        _map.CompressBounds();
    }

    public void Rotate(RotateDirection rotation)
    {
        int oldRows = _map.size.y;
        int oldCols = _map.size.x;

        TileBase[] old = Tiles;

        TileBase[] aux = new TileBase[oldRows * oldCols];
        for(int rowIdx = 0; rowIdx < oldCols; ++rowIdx)
        {
            int currentRowBase = rowIdx * oldRows;
            for(int colIdx = 0; colIdx < oldRows; ++colIdx)
            {
                int oldIdx = 0;
                if (rotation == RotateDirection.Left)
                {
                    oldIdx = rowIdx + oldCols * (oldRows - colIdx - 1);
                }
                else if (rotation == RotateDirection.Right)
                {
                    oldIdx = oldCols * colIdx + oldCols - rowIdx - 1;
                }
                aux[currentRowBase + colIdx] = old[oldIdx];
            }
        }
        _map.ClearAllTiles();
        var newBounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(oldRows, oldCols, 1));
        _map.SetTilesBlock(newBounds, aux);
        RefreshBounds();
    }
}
