using UnityEngine;

/// <summary>
/// Pistola: Arma básica con munición infinita.
/// Semi-automática, disparo simple, bajo daño.
/// </summary>
public class Pistol : Weapon
{
    protected override void PerformShot()
    {
        // Dirección del disparo (centro de la pantalla/cámara)
        Vector3 shootDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        
        // Disparar raycast
        if (FireRaycast(out RaycastHit hit, shootDirection))
        {
            // Crear efecto de impacto
            CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
            
            // Aplicar daño
            ApplyDamage(hit.collider.gameObject, weaponData.damage);
            
            // Debug visual
            Debug.DrawLine(muzzlePoint.position, hit.point, Color.yellow, 0.1f);
        }
        else
        {
            // Disparo al aire (no impactó nada)
            Debug.DrawRay(muzzlePoint.position, shootDirection * weaponData.range, Color.red, 0.1f);
        }
    }
}