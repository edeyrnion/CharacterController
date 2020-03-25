using UnityEngine;


public class ShowGizmo : MonoBehaviour
{
    private enum GizmoType { LINE, SPHERE, CUBE };

    [SerializeField] private GizmoType gizmoType = GizmoType.SPHERE;
    [SerializeField] private Color gizmoColor = Color.black;
    [SerializeField] private float gizmoSize = 1;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        switch (gizmoType)
        {
            case GizmoType.LINE:
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoSize);
                break;
            case GizmoType.SPHERE:
                Gizmos.DrawWireSphere(transform.position, gizmoSize);
                break;
            case GizmoType.CUBE:
                Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
                break;
            default:
                break;
        }
    }
}
