using UnityEngine;

namespace Mirrro.VectorFieldBaker
{
    public static class GizmosUtility
    {
        public static void DrawGizmoArrow(Vector3 from, Vector3 direction, float arrowLength = 1f, float headLength = 0.05f,
            float headAngle = 20f)
        {
            Vector3 to = from + direction.normalized * arrowLength;
        
            Gizmos.DrawLine(from, to);
        
            if (direction != Vector3.zero)
            {
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + headAngle, 0) *
                                Vector3.forward;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - headAngle, 0) *
                               Vector3.forward;

                Gizmos.DrawLine(to, to + right * headLength);
                Gizmos.DrawLine(to, to + left * headLength);
            }
        }
    }
}