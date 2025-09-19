using System;
using UnityEngine;

[System.Serializable]
public class LineOfSight : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float viewHeight = 1.5f;
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
}
