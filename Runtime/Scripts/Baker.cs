using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Mirrro.VectorFieldBaker
{
    public static class Baker
    {
        private static int cellsX, cellsY, cellsZ;
    
        public static void BakeToAsset(Bounds bounds, float cellSize, VectorFieldAsset asset)
        {
            CellInfo[,,] grid = Generate(bounds, cellSize);
            List<VectorFieldEntry> entries = new();

            for (int x = 0; x < cellsX; x++)
            for (int y = 0; y < cellsY; y++)
            for (int z = 0; z < cellsZ; z++)
            {
                var cell = grid[x, y, z];
                if (!cell.escapeVector.hasVector)
                    continue;

                entries.Add(new VectorFieldEntry
                {
                    position = cell.center,
                    direction = cell.escapeVector.direction
                });
            }

            asset.boundsMin = bounds.min;
            asset.cellSize = cellSize;
            asset.entries = entries.ToArray();
        
            asset.BuildLookup();
        }

        public static DebugInfo Preview(Bounds bounds, float cellSize)
        {
            return new DebugInfo(FlattenGrid(Generate(bounds, cellSize)));
        }

        private static List<CellInfo> FlattenGrid(CellInfo[,,] grid)
        {
            List<CellInfo> list = new List<CellInfo>();

            int sizeX = grid.GetLength(0);
            int sizeY = grid.GetLength(1);
            int sizeZ = grid.GetLength(2);

            for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            for (int z = 0; z < sizeZ; z++)
            {
                list.Add(grid[x, y, z]);
            }

            return list;
        }

        public class DebugInfo
        {
            public List<CellInfo> CellInfos;

            public DebugInfo(List<CellInfo> cellInfos)
            {
                CellInfos = cellInfos;
            }
        }

        private static CellInfo[,,] Generate(Bounds bounds, float cellSize)
        {
            var cells = GenerateCellInfo(bounds, cellSize);
            GenerateVectors(cells);
            FloodEscapeVectors(cells);
            return cells;
        }

        private static void FloodEscapeVectors(CellInfo[,,] grid)
        {
            Queue<(int x, int y, int z)> cellsWithEscapePlan = new();
            HashSet<(int x, int y, int z)> cellsWithoutEscapePlan = new(); // faster lookup
        
            for (int x = 0; x < cellsX; x++)
            for (int y = 0; y < cellsY; y++)
            for (int z = 0; z < cellsZ; z++)
            {
                var cell = grid[x, y, z];
                if (!cell.isInside)
                    continue;

                if (cell.escapeVector.hasVector)
                    cellsWithEscapePlan.Enqueue((x, y, z));
                else
                    cellsWithoutEscapePlan.Add((x, y, z));
            }
        
            // Cells with escape plan will loop through their neighbors without an escape plan and pass on the plan. Then they can be removed from the loop.
            // The cells without an escape plan will receive all the plans and take an average of it. The ones who just received the plan will be the new cells with an escape plan.
            // Repeat until no cells are left that don't have an escape plan.
        
            while (cellsWithoutEscapePlan.Count > 0)
            {
                Dictionary<(int x, int y, int z), List<Vector3>> incomingVectors = new();
            
                int count = cellsWithEscapePlan.Count;
                for (int i = 0; i < count; i++)
                {
                    var (x, y, z) = cellsWithEscapePlan.Dequeue();
                    var sender = grid[x, y, z];

                    for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0) continue;

                        int nx = x + dx;
                        int ny = y + dy;
                        int nz = z + dz;

                        if (nx < 0 || ny < 0 || nz < 0 || nx >= cellsX || ny >= cellsY || nz >= cellsZ)
                            continue;

                        var neighbor = grid[nx, ny, nz];
                        var key = (nx, ny, nz);

                        if (!neighbor.isInside || neighbor.escapeVector.hasVector)
                            continue;

                        if (!cellsWithoutEscapePlan.Contains(key))
                            continue;

                        if (!incomingVectors.ContainsKey(key))
                            incomingVectors[key] = new List<Vector3>();
                    
                        Vector3 dir = (sender.center - neighbor.center).normalized;
                        incomingVectors[key].Add(dir);
                    }
                
                    cellsWithEscapePlan.Enqueue((x, y, z));
                }
            
                foreach (var kvp in incomingVectors)
                {
                    var (x, y, z) = kvp.Key;
                    var cell = grid[x, y, z];

                    Vector3 average = Vector3.zero;
                    foreach (var v in kvp.Value)
                        average += v;

                    average /= kvp.Value.Count;
                    average.Normalize();

                    cell.escapeVector.direction = average;
                    cell.escapeVector.hasVector = true;

                    cellsWithEscapePlan.Enqueue(kvp.Key);
                    cellsWithoutEscapePlan.Remove(kvp.Key);
                }
            
                if (incomingVectors.Count == 0)
                {
                    Debug.LogWarning(
                        "Some inner cells could not receive any escape vector. Might be fully enclosed with no initial frontier.");
                    break;
                }
            }
        }

        private static void GenerateVectors(CellInfo[,,] cellInfos)
        {
            for (int x = 0; x < cellsX; x++)
            for (int y = 0; y < cellsY; y++)
            for (int z = 0; z < cellsZ; z++)
            {
                CellInfo current = cellInfos[x, y, z];

                if (!current.isInside)
                    continue;

                List<Vector3> directions = new List<Vector3>();
            
                for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0)
                    {
                        continue;
                    }
                
                    int nx = x + dx;
                    int ny = y + dy;
                    int nz = z + dz;

                    if (nx < 0 || ny < 0 || nz < 0 || nx >= cellsX || ny >= cellsY || nz >= cellsZ)
                    {
                        continue;
                    }


                    CellInfo neighbor = cellInfos[nx, ny, nz];

                    if (!neighbor.isInside)
                    {
                        directions.Add((neighbor.center - current.center).normalized);
                    }
                }

                if (directions.Count > 0)
                {
                    Vector3 direction = Vector3.zero;

                    foreach (var dir in directions)
                    {
                        direction += dir;
                    }

                    direction /= directions.Count;
                    current.escapeVector.direction = direction.normalized;
                    current.escapeVector.hasVector = true;
                }
            }
        }

        private static CellInfo[,,] GenerateCellInfo(Bounds bounds, float cellSize)
        {
            Vector3[,,] centers = GenerateCellCenters(bounds, cellSize);
            CellInfo[,,] cellInfos = new CellInfo[cellsX, cellsY, cellsZ];
        
            for (int x = 0; x < cellsX; x++)
            for (int y = 0; y < cellsY; y++)
            for (int z = 0; z < cellsZ; z++)
            {
                Vector3 cellCenter = centers[x, y, z];
                CellInfo cellInfo = new CellInfo();
                cellInfo.center = cellCenter;

                Collider[] results = new Collider[1];
                int hitCount = Physics.OverlapBoxNonAlloc(cellCenter, Vector3.one * (cellSize / 2f), results);

                if (hitCount > 0)
                {
                    cellInfo.isInside = true;
                }

                cellInfos[x, y, z] = cellInfo;
            }

            return cellInfos;
        }

        private static Vector3[,,] GenerateCellCenters(Bounds bounds, float cellSize)
        {
            Vector3 size = bounds.size;
            cellsX = Mathf.CeilToInt(size.x / cellSize);
            cellsY = Mathf.CeilToInt(size.y / cellSize);
            cellsZ = Mathf.CeilToInt(size.z / cellSize);

            Vector3[,,] centers = new Vector3[cellsX, cellsY, cellsZ];
            Vector3 origin = bounds.min + Vector3.one * (cellSize * 0.5f);

            for (int x = 0; x < cellsX; x++)
            for (int y = 0; y < cellsY; y++)
            for (int z = 0; z < cellsZ; z++)
            {
                centers[x, y, z] = origin + new Vector3(x * cellSize, y * cellSize, z * cellSize);
            }

            return centers;
        }
    }
}

public class VectorInformation
{
    public Vector3 direction = Vector3.zero;
    public bool hasVector = false;
}

public class CellInfo
{
    public Vector3 center;
    public bool isInside = false;
    public readonly VectorInformation escapeVector = new();
}