using UnityEngine;

/// <summary>
/// Rifle: Arma automática de alta cadencia.
/// Daño medio, disparo rápido, puede tener ligera dispersión.
/// </summary>
public class Rifle : Weapon
{
    protected override void PerformShot()
    {
        Vector3 baseDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        
        // Aplicar ligera dispersión si está configurada
        Vector3 shootDirection = GetSpreadDirection(baseDirection);
        
        // Disparar raycast
        if (FireRaycast(out RaycastHit hit, shootDirection))
        {
            // Crear efecto de impacto
            CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
            
            // Aplicar daño
            ApplyDamage(hit.collider.gameObject, weaponData.damage);
            
            // Debug visual
            Debug.DrawLine(muzzlePoint.position, hit.point, Color.cyan, 0.1f);
        }
        else
        {
            Debug.DrawRay(muzzlePoint.position, shootDirection * weaponData.range, Color.blue, 0.1f);
        }
    }
}