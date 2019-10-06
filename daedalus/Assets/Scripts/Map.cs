using UnityEngine;
using System.Collections;

using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class Map : MonoBehaviour
{
    [SerializeField] TileSet _tileset;
    [SerializeField] Tilemap _tilemap;

    Vector3Int[] _neighbourOffsets = new Vector3Int[]
    {
        new Vector3Int(0,1,0), new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0)
    };

    public BoundsInt CellBounds => _tilemap.cellBounds;

    public bool HasTileAt(Vector3Int coords) => _tilemap.HasTile(coords);

    Vector3 _halfCellSize;
    
    public void InitTilesFromView()
    {
        // Cache all logic-related data
        _tilemap.CompressBounds();
        _halfCellSize = _tilemap.cellSize;
        _halfCellSize.Scale(new Vector3(0.5f, 0.5f, 1.0f));

    }

    public void InitFromAsset()//.....)
    {

    }

    public void GetNeighbourDeltas(Vector3Int currentCoords, out Vector3Int[] offsets)
    {
        var source = _neighbourOffsets;
        int deltasLen = source.Length;
        offsets = new Vector3Int[deltasLen];
        Array.Copy(source, 0, offsets, 0, deltasLen);
    }

    public TileData GetTileDataAt(Vector3Int pos)
    {
        var tile = _tilemap.GetTile(pos);
        return _tileset.GetByTile(tile);
    }

    public TileType GetTileTypeAt(Vector3Int pos)
    {
        return GetTileDataAt(pos)?.TileType ?? TileType.None;
    }

    public bool AreTilesConnected(Vector3Int from, Vector3Int to)
    {
        return false;
    }

    public bool TrySetBlock(PlaceableBlock block, Vector3Int coords)
    {
        if(CanSetBlock(block, coords))
        {
            var targetBounds = block.Bounds;
            var size = targetBounds.size;
            targetBounds.xMin = coords.x;
            targetBounds.yMin = coords.y;
            targetBounds.size = size;
            _tilemap.SetTilesBlock(targetBounds, block.Tiles);
            return true;
        }
        return false;
    }

    public bool CanSetBlock(PlaceableBlock block, Vector3Int coords)
    {
        BoundsInt blockBounds = block.Bounds; // who would've guessed :D
        for(int x = blockBounds.xMin; x < blockBounds.xMin + blockBounds.size.x; ++x)
        {
            for(int y = blockBounds.yMin; y < blockBounds.yMin + blockBounds.size.y; ++y)
            {
                var blockTileAt = block.TileAt(x, y);
                if(blockTileAt == null || _tileset.GetByTile(blockTileAt).TileType == TileType.None)
                {
                    continue;
                }
                var tileTargetCoords = coords;
                tileTargetCoords.x += x;
                tileTargetCoords.y += y;
                var tileData = GetTileDataAt(tileTargetCoords);
                if(tileData != null && tileData.BlocksPlacement)
                {
                    return false;
                }
            }
        }

        return true;
    }

    internal void RefreshHiddenTiles(Vector3Int playerCoords)
    {
        List<Vector3Int> setHidden = new List<Vector3Int>();
        List<Vector3Int> setVisible = new List<Vector3Int>();
        foreach(var tilePos in _tilemap.cellBounds.allPositionsWithin)
        {
            TileData data = GetTileDataAt(tilePos);
            var type = data?.TileType ?? TileType.None;

            if (type == TileType.None || type == TileType.Wall)
            {
                continue;
            }

            if(type == TileType.Hidden && ExistsPlayerWalkablePath(playerCoords, tilePos))
            {
                setVisible.Add(tilePos);
            }
            else if (type == TileType.Ground && !ExistsPlayerWalkablePath(playerCoords, tilePos))
            {
                setHidden.Add(tilePos);
            }
        }

        var hiddenTile = _tileset.GetByType(TileType.Hidden);
        foreach(var coord in setHidden)
        {
            _tilemap.SetTile(coord, hiddenTile.UnityTile);
        }
        var visibleTile = _tileset.GetByType(TileType.Ground);
        foreach (var coord in setVisible)
        {
            _tilemap.SetTile(coord, visibleTile.UnityTile);
        }
    }

    public Vector3Int CoordsFromWorld(Vector3 worldPos)
    {
        return _tilemap.WorldToCell(worldPos);
    }

    public Vector3 WorldFromCoords(Vector3Int coords, bool centered)
    {
        var pos = _tilemap.CellToWorld(coords);
        if(centered)
        {
            pos += _halfCellSize;
        }
        return pos;
    }

    public bool EntityCanMoveTo(Vector3Int coords)
    {
        var tile = _tilemap.GetTile(coords);
        return (tile != null && _tileset.GetByTile(tile).PlayerWalkable);
    }

    public  Vector3Int ClampBlockToFitBounds(BoundsInt sourceVector)
    {
        Vector3Int targetVector = new Vector3Int(sourceVector.xMin, sourceVector.yMin, 0);
        if (targetVector.x < 0) targetVector.x = 0;
        if (targetVector.y < 0) targetVector.y = 0;
        int w = sourceVector.size.x;
        int h = sourceVector.size.y;
        if (targetVector.x + w >= _tilemap.size.x)
        {
            targetVector.x = _tilemap.size.x - w;
        }
        if(targetVector.y + h >= _tilemap.size.y)
        {
            targetVector.y = _tilemap.size.y - h;
        }
        return targetVector;
    }

    public bool ExistsPlayerWalkablePath(Vector3Int from, Vector3Int to)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        if (from.Equals(to))
            return true; // Trivial, don't waste time

        PathUtils.FindPath(this, from, to, (coords) => TestPlayerWalkable(coords) || coords.Equals(to), ref path);
        return path.Count >= 2;
    }

    public bool TestPlayerWalkable(Vector3Int coords)
    {
        return GetTileDataAt(coords)?.PlayerWalkable ?? false;
    }
}
