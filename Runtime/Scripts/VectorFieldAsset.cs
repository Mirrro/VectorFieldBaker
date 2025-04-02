using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirrro.VectorFieldBaker
{
    [CreateAssetMenu(fileName = "VectorFieldAsset", menuName = "Mirrro/Vector Field")]
    public class VectorFieldAsset : ScriptableObject
    {
        public Vector3 boundsMin;
        public float cellSize;

        public VectorFieldEntry[] entries;

        private Dictionary<Vector3Int, Vector3> lookup;

        public void BuildLookup()
        {
            lookup = new Dictionary<Vector3Int, Vector3>();
            foreach (var entry in entries)
            {
                var gridPos = WorldToGrid(entry.position);
                lookup[gridPos] = entry.direction;
            }
        }

        public bool TryGetDirection(Vector3 worldPos, out Vector3 direction)
        {
            direction = Vector3.zero;

            var gridPos = WorldToGrid(worldPos);
            if (lookup != null && lookup.TryGetValue(gridPos, out direction))
            {
                return true;
            }

            return false;
        }

        private Vector3Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 local = worldPos - boundsMin;
            return new Vector3Int(
                Mathf.FloorToInt(local.x / cellSize),
                Mathf.FloorToInt(local.y / cellSize),
                Mathf.FloorToInt(local.z / cellSize)
            );
        }
    }
    
    [Serializable]
    public struct VectorFieldEntry
    {
        public Vector3 position;
        public Vector3 direction;
    }
}
