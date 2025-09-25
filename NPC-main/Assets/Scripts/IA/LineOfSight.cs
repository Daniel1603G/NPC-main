using System;
using UnityEngine;

[System.Serializable]
public class LineOfSight : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float viewHeight = 0f;
    [SerializeField] private LayerMask obstructionMask;

    public float DetectionRange
    {
        get => detectionRange;
        set => detectionRange = value;
    }

    public bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 origin = transform.position + Vector3.up * viewHeight;
        Vector3 targetPos = target.position + Vector3.up * viewHeight;
        Vector3 direction = targetPos - origin;
        float distance = direction.magnitude;

        if (distance > detectionRange) return false;

        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > fieldOfViewAngle * 0.5f) return false;

        Vector3 dirNorm = direction / distance;
        if (Physics.Raycast(origin, dirNorm, out RaycastHit hit, distance, obstructionMask))
        {
            if (!hit.transform.IsChildOf(target) && hit.transform != target)
            {
                return false;
            }
        }

        var spin = target.GetComponent<ISpin>();
        if (spin != null && !spin.IsDetectable)
        {
            return false;
        }

        return true;
    }


    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * viewHeight;

        Color rangeColor = new Color(0f, 0.6f, 1f, 0.35f);
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(origin, detectionRange);

        Gizmos.color = Color.red;
        Vector3 leftDir = Quaternion.Euler(0f, +fieldOfViewAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, -fieldOfViewAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(origin, leftDir * detectionRange);
        Gizmos.DrawRay(origin, rightDir * detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, transform.forward * detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(origin, 0.05f);
    }
}
