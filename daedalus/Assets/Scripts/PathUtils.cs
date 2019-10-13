using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathUtils
{
    private class PathInfo
    {
        public Vector3Int Coords;
        public Vector3Int? From;
        public int Distance;

        public PathInfo(Vector3Int coords, Vector3Int? from = null, int dist = Int32.MaxValue)
        {
            Coords = coords;
            From = from;
            Distance = dist;
        }

        public override string ToString() => $"{Coords} <- {(From.HasValue ? From.Value.ToString() : "NONE")} [{Distance}]";
    }

    public static void FindPath(Map map, Vector3Int from, Vector3Int to, Predicate<Vector3Int> walkabilityTest, ref List<Vector3Int> path)
    {
        path.Clear();
        if (from == to)
        {
            path.Add(to);
            return;
        }

        Dictionary<Vector3Int, PathInfo> visitedInfo = new Dictionary<Vector3Int, PathInfo>();
        PriorityQueue<Vector3Int> coordsQueue = new PriorityQueue<Vector3Int>();
        BoundsInt mapBounds = map.CellBounds;
        List<Vector3Int> validTiles = new List<Vector3Int>();
        foreach (var position in mapBounds.allPositionsWithin)
        {
            if (map.HasTileAt(position) && walkabilityTest(position))
            {
                validTiles.Add(position);
                visitedInfo[position] = new PathInfo(position);
                coordsQueue.Enqueue(position, visitedInfo[position].Distance);
            }
        }        
        visitedInfo[from].Distance = 0;
        coordsQueue.UpdateKey(from, visitedInfo[from].Distance);

        while (coordsQueue.Count > 0)
        {
            Vector3Int currentCoords = coordsQueue.Dequeue();
            Vector3Int[] deltas;
            map.GetNeighbourDeltas(currentCoords, out deltas);
            PathInfo currentInfo = visitedInfo[currentCoords];
            int formerDistance = currentInfo.Distance;
            if(formerDistance == Int32.MaxValue)
            {
                break;
            }
            foreach (var delta in deltas)
            {
                Vector3Int neighbourCoords = currentCoords + delta;
                if (!visitedInfo.ContainsKey(neighbourCoords))
                {
                    continue;
                }

                // TODO: Other checks: Doors, etc, etc.
                PathInfo neighbourInfo = visitedInfo[neighbourCoords];
                int distance = formerDistance + 1;
                if (distance < neighbourInfo.Distance)
                {
                    neighbourInfo.From = currentCoords;
                    neighbourInfo.Distance = distance;

                    int count = coordsQueue.Count;
                    if (coordsQueue.Count > 0)
                    {
                        coordsQueue.UpdateKey(neighbourCoords, distance);
                    }
                    if (count != coordsQueue.Count)
                    {
                        Debug.LogError("Pathfinding issue. count doesn't match queue length (impossibru!)");
                    }
                }
            }
        }

        if (visitedInfo.TryGetValue(to, out var destinationInfo) && destinationInfo.Distance != Int32.MaxValue)
        {
            List<Vector3Int> rList = new List<Vector3Int>();
            rList.Add(to);
            while (destinationInfo.From.HasValue)
            {
                rList.Add(destinationInfo.From.Value);
                destinationInfo = visitedInfo[destinationInfo.From.Value];
            }

            for (int i = rList.Count - 1; i >= 0; i--)
            {
                path.Add(rList[i]);
            }
        }
        else
        {
            path.Add(from);
        }        
    }
}
