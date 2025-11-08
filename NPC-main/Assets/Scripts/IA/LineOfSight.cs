using System;
using UnityEngine;
using Random = UnityEngine.Random;


[System.Serializable]
public class LineOfSight : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float viewHeight = 0f;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Dynamic Vision (NEW)")]
    [Tooltip("Habilita variación aleatoria en la visión")]
    [SerializeField] private bool enableDynamicVision = true;
    [Tooltip("Variación máxima del rango (+/- porcentaje)")]
    [SerializeField, Range(0f, 0.5f)] private float rangeVariation = 0.2f; // 20%
    [Tooltip("Variación del campo de visión (+/- grados)")]
    [SerializeField, Range(0f, 30f)] private float fovVariation = 15f;
    [Tooltip("Frecuencia de cambio (segundos)")]
    [SerializeField] private float variationInterval = 3f;
    
    private float currentRangeMultiplier = 1f;
    private float currentFOVOffset = 0f;
    private float lastVariationTime;
    
    public float DetectionRange
    {
        get => detectionRange;
        set => detectionRange = value;
    }

    private void Start()
    {
        lastVariationTime = Time.time;
        UpdateVisionVariation();
    }
    
    private void Update()
    {
        if (enableDynamicVision && Time.time - lastVariationTime > variationInterval)
        {
            UpdateVisionVariation();
            lastVariationTime = Time.time;
        }
    }
    
    /// <summary>
    /// Actualiza la variación aleatoria de visión.
    /// Simula que el enemigo está más o menos alerta.
    /// </summary>
    private void UpdateVisionVariation()
    {
        // Variación del rango: entre (1 - variation) y (1 + variation)
        currentRangeMultiplier = 1f + Random.Range(-rangeVariation, rangeVariation);
        
        // Variación del FOV: entre -fovVariation y +fovVariation
        currentFOVOffset = Random.Range(-fovVariation, fovVariation);
    }

    public bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        Vector3 origin = transform.position + Vector3.up * viewHeight;
        Vector3 targetPos = target.position + Vector3.up * viewHeight;
        Vector3 direction = targetPos - origin;
        float distance = direction.magnitude;

        // Aplicar variación de rango
        float effectiveRange = detectionRange * currentRangeMultiplier;
        if (distance > effectiveRange) return false;

        // Aplicar variación de FOV
        float effectiveFOV = fieldOfViewAngle + currentFOVOffset;
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > effectiveFOV * 0.5f) return false;

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

        // Rango efectivo con variación
        float effectiveRange = enableDynamicVision ? detectionRange * currentRangeMultiplier : detectionRange;
        Color rangeColor = new Color(0f, 0.6f, 1f, 0.35f);
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(origin, effectiveRange);

        // FOV efectivo con variación
        float effectiveFOV = enableDynamicVision ? fieldOfViewAngle + currentFOVOffset : fieldOfViewAngle;
        Gizmos.color = Color.red;
        Vector3 leftDir = Quaternion.Euler(0f, +effectiveFOV * 0.5f, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, -effectiveFOV * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(origin, leftDir * effectiveRange);
        Gizmos.DrawRay(origin, rightDir * effectiveRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, transform.forward * effectiveRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(origin, 0.05f);
    }
}