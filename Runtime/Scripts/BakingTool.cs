using UnityEngine;

namespace Mirrro.VectorFieldBaker
{
    public class BakingTool : MonoBehaviour
    {
        [SerializeField] private VectorFieldAsset vectorFieldAsset;
        [SerializeField] private Bounds bounds;
        [SerializeField] private float cellSize;
    
        public void Start()
        {
            if (vectorFieldAsset == null)
            {
                return;
            }
        
            Baker.BakeToAsset(bounds, cellSize, vectorFieldAsset);
        }

        private void OnDrawGizmosSelected()
        {
            if (cellSize < 0)
            {
                return;
            }
            
            var debugInfo = Baker.Preview(bounds, cellSize);
        
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
    
            var grid = debugInfo.CellInfos;

            foreach (var cellInfo in grid)
            {
                Gizmos.color = cellInfo.escapeVector.hasVector ? Color.magenta :
                    cellInfo.isInside ? Color.red : new Color(0, 1, 0, 0.3f);
                GizmosUtility.DrawGizmoArrow(cellInfo.center, cellInfo.escapeVector.direction, cellSize * 0.5f);
            }
        }
    }
}