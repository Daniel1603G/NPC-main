using UnityEngine;


public class Rifle : Weapon
{
    protected override void PerformShot()
    {
        Vector3 baseDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;
        Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
        
  
        Vector3 shootDirection = GetSpreadDirection(baseDirection);
        
    
        if (FireRaycast(out RaycastHit hit, shootDirection))
        {
    
            CreateBulletTracer(origin, hit.point);
            
          
            CreateImpactEffect(hit.point, Quaternion.LookRotation(hit.normal));
            
          
            ApplyDamage(hit.collider.gameObject, weaponData.damage);
            
            
            Debug.DrawLine(origin, hit.point, Color.cyan, 0.1f);
        }
        else
        {
            Vector3 endPoint = origin + shootDirection * weaponData.range;
            CreateBulletTracer(origin, endPoint);
            Debug.DrawRay(origin, shootDirection * weaponData.range, Color.blue, 0.1f);
        }
    }
}