using UnityEngine;

/// <summary>
/// Escopeta: Dispara múltiples proyectiles con dispersión.
/// Alto daño a corta distancia, munición limitada.
/// </summary>
public class Shotgun : Weapon
{
    protected override void PerformShot()
    {
        Vector3 baseDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        
        // Disparar múltiples proyectiles (pellets)
        for (int i = 0; i < weaponData.projectilesPerShot; i++)
        {
            // Aplicar dispersión a cada proyectil
            Vector3 shootDirection = GetSpreadDirection(baseDirection);
            
            // Disparar raycast
            if (FireRaycast(out RaycastHit hit, shootDirection))
            {
                // Crear efecto de impacto
                CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
                
                // Aplicar daño (dividido entre todos los proyectiles)
                float damagePerPellet = weaponData.damage / weaponData.projectilesPerShot;
                ApplyDamage(hit.collider.gameObject, damagePerPellet);
                
                // Debug visual
                Debug.DrawLine(muzzlePoint.position, hit.point, Color.red, 0.1f);
            }
            else
            {
                Debug.DrawRay(muzzlePoint.position, shootDirection * weaponData.range, Color.gray, 0.1f);
            }
        }
    }
}