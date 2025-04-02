using UnityEngine;

namespace Mirrro.VectorFieldBaker
{
    public class VectorAssetTest : MonoBehaviour
    {
        [SerializeField] private VectorField vectorField;
        public Vector3 direction = Vector3.zero;
        private void Update()
        {
            direction = 
                vectorField.TryGetDirection(transform.position, out Vector3 vector) 
                    ? vector 
                    : Vector3.zero;
        }
    
        private void OnDrawGizmos()
        {
            GizmosUtility.DrawGizmoArrow(transform.position, direction);
        }
    }
}