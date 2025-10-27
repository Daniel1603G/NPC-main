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
        Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
        
        // Disparar raycast
        if (FireRaycast(out RaycastHit hit, shootDirection))
        {
            // Crear tracer visual de la bala
            CreateBulletTracer(origin, hit.point);
            
            // Crear efecto de impacto
            CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
            
            // Aplicar daño
            ApplyDamage(hit.collider.gameObject, weaponData.damage);
            
            // Debug visual
            Debug.DrawLine(origin, hit.point, Color.yellow, 0.1f);
        }
        else
        {
            // Disparo al aire (no impactó nada)
            Vector3 endPoint = origin + shootDirection * weaponData.range;
            CreateBulletTracer(origin, endPoint);
            Debug.DrawRay(origin, shootDirection * weaponData.range, Color.red, 0.1f);
        }
    }
}