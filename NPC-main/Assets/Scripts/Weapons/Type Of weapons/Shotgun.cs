using UnityEngine;


public class Shotgun : Weapon
{
    protected override void PerformShot()
    {
        Vector3 baseDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
        
  
        for (int i = 0; i < weaponData.projectilesPerShot; i++)
        {
           
            Vector3 shootDirection = GetSpreadDirection(baseDirection);
           
            if (FireRaycast(out RaycastHit hit, shootDirection))
            {
              
                CreateBulletTracer(origin, hit.point);
                
             
                CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
                
              
                float damagePerPellet = weaponData.damage / weaponData.projectilesPerShot;
                ApplyDamage(hit.collider.gameObject, damagePerPellet);
                
           
                Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
            }
            else
            {
                Vector3 endPoint = origin + shootDirection * weaponData.range;
                CreateBulletTracer(origin, endPoint);
                Debug.DrawRay(origin, shootDirection * weaponData.range, Color.gray, 0.1f);
            }
        }
    }
}