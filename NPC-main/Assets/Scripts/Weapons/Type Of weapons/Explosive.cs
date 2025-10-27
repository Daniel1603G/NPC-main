using UnityEngine;

/// <summary>
/// Arma explosiva: Dispara proyectiles que explotan al impactar.
/// Daño en área, disparo con arco, munición limitada.
/// </summary>
public class Explosive : Weapon
{
    protected override void PerformShot()
    {
        if (weaponData.projectilePrefab == null)
        {
            Debug.LogWarning("No hay prefab de proyectil asignado en el arma explosiva!");
            return;
        }
        
        Vector3 spawnPosition = muzzlePoint != null ? muzzlePoint.position : transform.position;
        Vector3 shootDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        
        // Instanciar el proyectil
        GameObject projectileObj = Instantiate(weaponData.projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
        
        // Configurar el proyectil
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(weaponData.damage, weaponData.explosionRadius, weaponData.projectileSpeed, hitLayers);
            projectile.ImpactEffect = weaponData.impactEffect;
        }
        else
        {
            // Si no tiene componente Projectile, agregar Rigidbody y velocidad
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = projectileObj.AddComponent<Rigidbody>();
                
            rb.velocity = shootDirection * weaponData.projectileSpeed;
            
            // Destruir después de un tiempo
            Destroy(projectileObj, 5f);
        }
    }
}