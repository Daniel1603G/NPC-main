using UnityEngine;

/// <summary>
/// Sniper: Arma de precisión con alto daño y largo alcance.
/// Disparo lento, sin dispersión, máximo daño.
/// </summary>
public class Sniper : Weapon
{
    [Header("Sniper Specific")]
    [SerializeField] private bool canPenetrateTargets = false;
    [SerializeField] private int maxPenetrations = 2;
    
    protected override void PerformShot()
    {
        Vector3 shootDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
        
        if (!canPenetrateTargets)
        {
            // Disparo simple sin penetración
            if (FireRaycast(out RaycastHit hit, shootDirection))
            {
                CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
                ApplyDamage(hit.collider.gameObject, weaponData.damage);
                Debug.DrawLine(origin, hit.point, Color.green, 0.5f);
            }
        }
        else
        {
            // Disparo con penetración (puede atravesar múltiples objetivos)
            RaycastHit[] hits = Physics.RaycastAll(origin, shootDirection, weaponData.range, hitLayers);
            
            if (hits.Length > 0)
            {
                // Ordenar por distancia
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                
                int penetrationCount = 0;
                
                foreach (RaycastHit hit in hits)
                {
                    if (penetrationCount >= maxPenetrations)
                        break;
                    
                    CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
                    
                    // Reducir daño con cada penetración
                    float damageMultiplier = 1f - (penetrationCount * 0.3f);
                    ApplyDamage(hit.collider.gameObject, weaponData.damage * damageMultiplier);
                    
                    Debug.DrawLine(origin, hit.point, Color.green, 0.5f);
                    
                    penetrationCount++;
                }
            }
        }
    }
}