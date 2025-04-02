using UnityEngine;

namespace Mirrro.VectorFieldBaker
{
    public class VectorField : MonoBehaviour
    {
        [SerializeField] private VectorFieldAsset vectorFieldAsset;
        [SerializeField] private Mesh mesh;

        public bool TryGetDirection(Vector3 position, out Vector3 direction)
        {
            return vectorFieldAsset.TryGetDirection(position - transform.position, out direction);
        }
    
        private void OnDrawGizmosSelected()
        {
            if (vectorFieldAsset == null)
            {
                return;
            }

            if (mesh == null)
            {
                return;
            }
            
            foreach (var vectorFieldEntry in vectorFieldAsset.entries)
            {
                Gizmos.color = new Color(vectorFieldEntry.direction.x, vectorFieldEntry.direction.y, vectorFieldEntry.direction.z);
                var lookRot = Quaternion.LookRotation(vectorFieldEntry.direction.normalized, Vector3.up);
                Gizmos.DrawMesh(mesh, 0, transform.position + vectorFieldEntry.position, lookRot, Vector3.one);
            }
        }
    }
}